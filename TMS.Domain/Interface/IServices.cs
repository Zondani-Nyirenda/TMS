namespace TMS.Domain.Interfaces;

/// <summary>
/// Generates unique human-readable numbers for students, invoices, etc.
/// Implementation lives in Infrastructure.
/// </summary>
public interface INumberGeneratorService
{
    Task<string> GenerateStudentNumberAsync(CancellationToken ct = default);
    Task<string> GenerateTutorStaffNumberAsync(CancellationToken ct = default);
    Task<string> GenerateInvoiceNumberAsync(CancellationToken ct = default);
    Task<string> GenerateReceiptNumberAsync(CancellationToken ct = default);
}

/// <summary>
/// Abstracts email sending. Implementation uses SMTP / SendGrid / etc.
/// </summary>
public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
    Task SendFeeReminderAsync(string to, string studentName, string invoiceNumber,
        decimal balanceAmount, DateTime dueDate, CancellationToken ct = default);
    Task SendWelcomeEmailAsync(string to, string fullName, string tempPassword,
        CancellationToken ct = default);
}

/// <summary>
/// Abstracts PWA push notifications.
/// </summary>
public interface IPushNotificationService
{
    Task SendAsync(string userId, string title, string body, string? actionUrl = null,
        CancellationToken ct = default);
    Task SendToRoleAsync(string role, string title, string body,
        CancellationToken ct = default);
}

/// <summary>
/// Abstracts file storage (local disk, Azure Blob, S3, etc.).
/// Infrastructure provides the concrete implementation.
/// </summary>
