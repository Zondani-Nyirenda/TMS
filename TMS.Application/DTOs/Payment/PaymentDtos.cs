using System.ComponentModel.DataAnnotations;
using TMS.Domain.Enums;

namespace TMS.Application.DTOs.Payment;

// ── Fee Structure ────────────────────────────────────────────────────────────

public class FeeStructureDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZMW";
    public bool IsRecurring { get; set; }
    public int? RecurringDayOfMonth { get; set; }
    public bool IsActive { get; set; }
}

public class CreateFeeStructureRequest
{
    [Required] public Guid CourseId { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Range(0, 10000000)] public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZMW";
    public bool IsRecurring { get; set; } = true;
    [Range(1, 31)] public int? RecurringDayOfMonth { get; set; }
}

// ── Invoice ──────────────────────────────────────────────────────────────────

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string Currency { get; set; } = "ZMW";
    public InvoiceStatus Status { get; set; }
    public string? Notes { get; set; }
    public List<InvoiceLineItemDto> LineItems { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
}

public class InvoiceLineItemDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal LineTotal { get; set; }
}

public class CreateInvoiceRequest
{
    [Required] public Guid StudentId { get; set; }
    [Required] public DateTime DueDate { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? Notes { get; set; }
    public string Currency { get; set; } = "ZMW";
    [Required, MinLength(1)] public List<CreateInvoiceLineItemRequest> LineItems { get; set; } = new();
}

public class CreateInvoiceLineItemRequest
{
    [Required] public string Description { get; set; } = string.Empty;
    [Range(1, 9999)] public int Quantity { get; set; } = 1;
    [Range(0, 10000000)] public decimal UnitPrice { get; set; }
    [Range(0, 10000000)] public decimal Discount { get; set; }
    public Guid? FeeStructureId { get; set; }
}

// ── Payment ──────────────────────────────────────────────────────────────────

public class PaymentDto
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZMW";
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

public class RecordPaymentRequest
{
    [Required] public Guid InvoiceId { get; set; }
    [Required, Range(0.01, 10000000)] public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZMW";
    [Required] public PaymentMethod Method { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

// ── Dashboard summary ────────────────────────────────────────────────────────

public class FinancialSummaryDto
{
    public decimal TotalInvoiced { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalOutstanding { get; set; }
    public decimal TotalOverdue { get; set; }
    public int InvoiceCount { get; set; }
    public int PaidInvoiceCount { get; set; }
    public int OverdueInvoiceCount { get; set; }
    public string Currency { get; set; } = "ZMW";
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
}

public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
