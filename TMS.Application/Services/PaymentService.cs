using AutoMapper;
using TMS.Application.Common;
using TMS.Application.DTOs.Payment;
using TMS.Application.Interfaces;
using TMS.Domain.Entities;
using TMS.Domain.Enums;
using TMS.Domain.Exceptions;
using TMS.Domain.Interfaces;

namespace TMS.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly INumberGeneratorService _numbers;

    public PaymentService(IUnitOfWork uow, IMapper mapper, INumberGeneratorService numbers)
    {
        _uow = uow;
        _mapper = mapper;
        _numbers = numbers;
    }

    // ── Invoices ──────────────────────────────────────────────────────────────

    public async Task<PagedResult<InvoiceDto>> GetInvoicesAsync(
        PaginationQuery query, Guid? studentId = null, CancellationToken ct = default)
    {
        var (items, total) = await _uow.Invoices.GetPagedAsync(
            pageNumber: query.PageNumber,
            pageSize: query.PageSize,
            predicate: studentId.HasValue ? i => i.StudentId == studentId.Value : null,
            orderBy: i => i.CreatedAt,
            descending: true,
            ct: ct);

        return PagedResult<InvoiceDto>.Create(
            _mapper.Map<List<InvoiceDto>>(items), total, query.PageNumber, query.PageSize);
    }

    public async Task<InvoiceDto> GetInvoiceByIdAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await _uow.Invoices.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Invoice), id);
        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(
        CreateInvoiceRequest request, string createdByUserId, CancellationToken ct = default)
    {
        var student = await _uow.Students.GetByIdAsync(request.StudentId, ct)
            ?? throw new NotFoundException(nameof(Student), request.StudentId);

        var lineItems = request.LineItems.Select(li => new InvoiceLineItem
        {
            Description = li.Description,
            Quantity = li.Quantity,
            UnitPrice = li.UnitPrice,
            Discount = li.Discount,
            FeeStructureId = li.FeeStructureId
        }).ToList();

        var subTotal = lineItems.Sum(li => li.LineTotal);
        var total = subTotal - request.DiscountAmount;

        var invoice = new Invoice
        {
            InvoiceNumber = await _numbers.GenerateInvoiceNumberAsync(ct),
            StudentId = request.StudentId,
            IssueDate = DateTime.UtcNow,
            DueDate = request.DueDate,
            SubTotal = subTotal,
            DiscountAmount = request.DiscountAmount,
            TotalAmount = total,
            PaidAmount = 0,
            Currency = request.Currency,
            Status = InvoiceStatus.Sent,
            Notes = request.Notes,
            CreatedByUserId = createdByUserId,
            LineItems = lineItems
        };

        await _uow.Invoices.AddAsync(invoice, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<InvoiceDto>(invoice);
    }

    // ── Payments ──────────────────────────────────────────────────────────────

    public async Task<PaymentDto> RecordPaymentAsync(
        RecordPaymentRequest request, string processedByUserId, CancellationToken ct = default)
    {
        var invoice = await _uow.Invoices.GetByIdAsync(request.InvoiceId, ct)
            ?? throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        if (request.Amount > invoice.BalanceAmount)
            throw new PaymentExceedsBalanceException(
                invoice.InvoiceNumber, invoice.BalanceAmount, request.Amount);

        var payment = new Payment
        {
            ReceiptNumber = await _numbers.GenerateReceiptNumberAsync(ct),
            StudentId = invoice.StudentId,
            InvoiceId = request.InvoiceId,
            Amount = request.Amount,
            Currency = request.Currency,
            PaymentDate = request.PaymentDate,
            Method = request.Method,
            Status = PaymentStatus.Paid,
            Reference = request.Reference,
            Notes = request.Notes,
            ProcessedByUserId = processedByUserId
        };

        // Update invoice paid amount and status
        invoice.PaidAmount += request.Amount;
        invoice.Status = invoice.BalanceAmount <= 0
            ? InvoiceStatus.Paid
            : InvoiceStatus.PartiallyPaid;

        await _uow.Payments.AddAsync(payment, ct);
        await _uow.Invoices.UpdateAsync(invoice, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<PaymentDto>(payment);
    }

    // ── Financial summary ─────────────────────────────────────────────────────

    public async Task<FinancialSummaryDto> GetFinancialSummaryAsync(
        int year, CancellationToken ct = default)
    {
        var invoices = await _uow.Invoices.FindAsync(
            i => i.CreatedAt.Year == year, ct);

        var payments = await _uow.Payments.FindAsync(
            p => p.PaymentDate.Year == year, ct);

        // Build monthly revenue
        var monthly = payments
            .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
            .Select(g => new MonthlyRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1)
                                .ToString("MMMM"),
                Amount = g.Sum(p => p.Amount)
            })
            .OrderBy(m => m.Month)
            .ToList();

        return new FinancialSummaryDto
        {
            TotalInvoiced = invoices.Sum(i => i.TotalAmount),
            TotalCollected = invoices.Sum(i => i.PaidAmount),
            TotalOutstanding = invoices.Sum(i => i.BalanceAmount),
            TotalOverdue = invoices
                .Where(i => i.DueDate < DateTime.UtcNow &&
                            i.Status != InvoiceStatus.Paid)
                .Sum(i => i.BalanceAmount),
            InvoiceCount = invoices.Count,
            PaidInvoiceCount = invoices.Count(i => i.Status == InvoiceStatus.Paid),
            OverdueInvoiceCount = invoices.Count(i =>
                                    i.DueDate < DateTime.UtcNow &&
                                    i.Status != InvoiceStatus.Paid),
            MonthlyRevenue = monthly
        };
    }

    // ── Fee structures ────────────────────────────────────────────────────────

    public async Task<List<FeeStructureDto>> GetFeeStructuresAsync(
        Guid? courseId = null, CancellationToken ct = default)
    {
        var list = courseId.HasValue
            ? await _uow.FeeStructures.FindAsync(f => f.CourseId == courseId.Value && f.IsActive, ct)
            : await _uow.FeeStructures.FindAsync(f => f.IsActive, ct);

        return _mapper.Map<List<FeeStructureDto>>(list);
    }

    public async Task<FeeStructureDto> CreateFeeStructureAsync(
        CreateFeeStructureRequest request, CancellationToken ct = default)
    {
        var course = await _uow.Courses.GetByIdAsync(request.CourseId, ct)
            ?? throw new NotFoundException(nameof(Course), request.CourseId);

        var fs = new FeeStructure
        {
            CourseId = request.CourseId,
            Name = request.Name,
            Description = request.Description,
            Amount = request.Amount,
            Currency = request.Currency,
            IsRecurring = request.IsRecurring,
            RecurringDayOfMonth = request.RecurringDayOfMonth,
            IsActive = true
        };

        await _uow.FeeStructures.AddAsync(fs, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<FeeStructureDto>(fs);
    }
}
