using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.Common;
using TMS.Application.DTOs.Payment;
using TMS.Application.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Manages invoices, payments, fee structures, and financial reporting.
/// </summary>
[Route("api/payments")]
public class PaymentsController : BaseController
{
    private readonly IPaymentService _payments;

    public PaymentsController(IPaymentService payments) => _payments = payments;

    // ── Invoices ──────────────────────────────────────────────────────────────

    /// <summary>Paginated list of invoices. Optionally filter by studentId.</summary>
    [HttpGet("invoices")]
    [Authorize(Roles = "Admin,Accountant")]
    [ProducesResponseType(typeof(PagedResult<InvoiceDto>), 200)]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] PaginationQuery query,
        [FromQuery] Guid? studentId,
        CancellationToken ct)
        => OkResult(await _payments.GetInvoicesAsync(query, studentId, ct));

    /// <summary>Invoice detail with line items and payments.</summary>
    [HttpGet("invoices/{id:guid}", Name = "GetInvoiceById")]
    [Authorize(Roles = "Admin,Accountant,Student")]
    [ProducesResponseType(typeof(InvoiceDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetInvoice(Guid id, CancellationToken ct)
        => OkResult(await _payments.GetInvoiceByIdAsync(id, ct));

    /// <summary>Create and issue a new invoice for a student.</summary>
    [HttpPost("invoices")]
    [Authorize(Roles = "Admin,Accountant")]
    [ProducesResponseType(typeof(InvoiceDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> CreateInvoice(
        [FromBody] CreateInvoiceRequest request, CancellationToken ct)
    {
        var invoice = await _payments.CreateInvoiceAsync(request, CurrentUserId, ct);
        return CreatedAtRoute("GetInvoiceById", new { id = invoice.Id },
            new { success = true, data = invoice });
    }

    // ── Payments ──────────────────────────────────────────────────────────────

    /// <summary>Record a payment against an invoice.</summary>
    [HttpPost("record")]
    [Authorize(Roles = "Admin,Accountant")]
    [ProducesResponseType(typeof(PaymentDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> RecordPayment(
        [FromBody] RecordPaymentRequest request, CancellationToken ct)
    {
        var payment = await _payments.RecordPaymentAsync(request, CurrentUserId, ct);
        return OkResult(payment, $"Payment {payment.ReceiptNumber} recorded.");
    }

    // ── Fee Structures ────────────────────────────────────────────────────────

    /// <summary>All active fee structures, optionally filtered by course.</summary>
    [HttpGet("fee-structures")]
    [Authorize(Roles = "Admin,Accountant")]
    [ProducesResponseType(typeof(List<FeeStructureDto>), 200)]
    public async Task<IActionResult> GetFeeStructures(
        [FromQuery] Guid? courseId, CancellationToken ct)
        => OkResult(await _payments.GetFeeStructuresAsync(courseId, ct));

    /// <summary>Create a fee structure for a course.</summary>
    [HttpPost("fee-structures")]
    [Authorize(Roles = "Admin,Accountant")]
    [ProducesResponseType(typeof(FeeStructureDto), 201)]
    public async Task<IActionResult> CreateFeeStructure(
        [FromBody] CreateFeeStructureRequest request, CancellationToken ct)
    {
        var fs = await _payments.CreateFeeStructureAsync(request, ct);
        return StatusCode(201, new { success = true, data = fs });
    }

    // ── Financial Summary ─────────────────────────────────────────────────────

    /// <summary>Financial summary for a given year — revenue, outstanding, overdue.</summary>
    [HttpGet("summary")]
    [Authorize(Roles = "Admin,Accountant")]
    [ProducesResponseType(typeof(FinancialSummaryDto), 200)]
    public async Task<IActionResult> GetFinancialSummary(
        [FromQuery] int? year, CancellationToken ct)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var summary = await _payments.GetFinancialSummaryAsync(targetYear, ct);
        return OkResult(summary);
    }
}