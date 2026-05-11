using TMS.Domain.Common;
using TMS.Domain.Enums;

namespace TMS.Domain.Entities;

/// <summary>
/// Fee structure for a course — can have multiple tiers (e.g. monthly, once-off).
/// </summary>
public class FeeStructure : BaseEntity
{
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public string Name { get; set; } = string.Empty;         // e.g. "Monthly Tuition"
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZMW";
    public bool IsRecurring { get; set; } = true;
    public int? RecurringDayOfMonth { get; set; }             // e.g. 1 = 1st of month
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Invoice issued to a student for fees owed.
/// </summary>
public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;  // e.g. INV-2024-0001
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount => TotalAmount - PaidAmount;

    public string Currency { get; set; } = "ZMW";
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public string? Notes { get; set; }
    public string? CreatedByUserId { get; set; }

    // Navigation
    public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

/// <summary>
/// A line item on an invoice (e.g. "Mathematics – January 2024").
/// </summary>
public class InvoiceLineItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal LineTotal => (UnitPrice * Quantity) - Discount;

    public Guid? FeeStructureId { get; set; }
    public FeeStructure? FeeStructure { get; set; }
}

/// <summary>
/// A payment made by or on behalf of a student.
/// </summary>
public class Payment : BaseEntity
{
    public string ReceiptNumber { get; set; } = string.Empty;  // e.g. RCP-2024-0001

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZMW";
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Paid;

    public string? Reference { get; set; }          // Bank ref / mobile money ref
    public string? Notes { get; set; }
    public string? ProcessedByUserId { get; set; }
}
