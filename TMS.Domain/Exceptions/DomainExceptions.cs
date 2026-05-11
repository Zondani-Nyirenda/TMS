namespace TMS.Domain.Exceptions;

/// <summary>
/// Thrown when a domain business rule is violated.
/// API layer maps this to HTTP 422 Unprocessable Entity.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when a requested entity does not exist.
/// API layer maps this to HTTP 404.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.") { }
}

/// <summary>
/// Thrown when a user attempts an unauthorised action.
/// API layer maps this to HTTP 403.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(message) { }
}

/// <summary>
/// Thrown when a student tries to enroll in a full class.
/// </summary>
public class ClassFullException : DomainException
{
    public ClassFullException(string className)
        : base($"Cannot enroll: class '{className}' has reached maximum capacity.") { }
}

/// <summary>
/// Thrown when a payment exceeds the outstanding invoice balance.
/// </summary>
public class PaymentExceedsBalanceException : DomainException
{
    public PaymentExceedsBalanceException(string invoiceNumber, decimal balance, decimal attempted)
        : base($"Payment of {attempted:N2} exceeds outstanding balance of {balance:N2} on invoice {invoiceNumber}.") { }
}
