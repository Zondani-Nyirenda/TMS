using FluentValidation;
using TMS.Application.DTOs.Auth;
using TMS.Application.DTOs.Attendance;
using TMS.Application.DTOs.Course;
using TMS.Application.DTOs.Exam;
using TMS.Application.DTOs.Payment;
using TMS.Application.DTOs.Student;
using TMS.Application.DTOs.Tutor;

namespace TMS.Application.Validators;

// ── Auth ─────────────────────────────────────────────────────────────────────

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}

// ── Student ──────────────────────────────────────────────────────────────────

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.PhoneNumber).NotEmpty().Matches(@"^\+?[0-9\s\-]{7,20}$")
            .WithMessage("Phone number is not valid.");
        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today.AddYears(-3))
            .WithMessage("Date of birth must be at least 3 years ago.")
            .GreaterThan(DateTime.Today.AddYears(-100))
            .WithMessage("Date of birth is unrealistically old.");
        RuleFor(x => x.GuardianName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.GuardianPhone).NotEmpty().Matches(@"^\+?[0-9\s\-]{7,20}$");
        RuleFor(x => x.GuardianRelationship).NotEmpty().MaximumLength(50);
        When(x => x.CreateLoginAccount, () =>
        {
            RuleFor(x => x.LoginPassword).NotEmpty().MinimumLength(8);
        });
    }
}

public class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
    public UpdateStudentRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.PhoneNumber).NotEmpty().Matches(@"^\+?[0-9\s\-]{7,20}$");
    }
}

// ── Tutor ─────────────────────────────────────────────────────────────────────

public class CreateTutorRequestValidator : AbstractValidator<CreateTutorRequest>
{
    public CreateTutorRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.PhoneNumber).NotEmpty();
        RuleFor(x => x.Qualification).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Specialization).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

// ── Course & Class ────────────────────────────────────────────────────────────

public class CreateCourseRequestValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20)
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("Course code must be uppercase letters, digits, and hyphens only.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.MaxCapacity).InclusiveBetween(1, 500);
        RuleFor(x => x.DurationWeeks).InclusiveBetween(1, 260);
        RuleFor(x => x.FeeAmount).GreaterThanOrEqualTo(0);
    }
}

public class CreateClassRequestValidator : AbstractValidator<CreateClassRequest>
{
    public CreateClassRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.TutorId).NotEmpty();
        RuleFor(x => x.StartDate).GreaterThanOrEqualTo(DateTime.Today.AddDays(-1))
            .WithMessage("Start date cannot be in the past.");
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time.");
        RuleFor(x => x.MaxCapacity).InclusiveBetween(1, 500);
    }
}

// ── Attendance ────────────────────────────────────────────────────────────────

public class SubmitAttendanceRequestValidator : AbstractValidator<SubmitAttendanceRequest>
{
    public SubmitAttendanceRequestValidator()
    {
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.Date).LessThanOrEqualTo(DateTime.UtcNow.AddHours(1))
            .WithMessage("Attendance date cannot be in the future.");
        RuleFor(x => x.Entries).NotEmpty().WithMessage("At least one attendance entry is required.");
        RuleForEach(x => x.Entries).ChildRules(entry =>
        {
            entry.RuleFor(e => e.StudentId).NotEmpty();
        });
    }
}

// ── Payment ───────────────────────────────────────────────────────────────────

public class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequest>
{
    public CreateInvoiceRequestValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.DueDate).GreaterThan(DateTime.Today)
            .WithMessage("Due date must be in the future.");
        RuleFor(x => x.LineItems).NotEmpty().WithMessage("Invoice must have at least one line item.");
        RuleForEach(x => x.LineItems).ChildRules(item =>
        {
            item.RuleFor(i => i.Description).NotEmpty().MaximumLength(300);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
    }
}

public class RecordPaymentRequestValidator : AbstractValidator<RecordPaymentRequest>
{
    public RecordPaymentRequestValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
        RuleFor(x => x.PaymentDate).LessThanOrEqualTo(DateTime.UtcNow.AddHours(1))
            .WithMessage("Payment date cannot be in the future.");
    }
}

// ── Exam ──────────────────────────────────────────────────────────────────────

public class CreateExamRequestValidator : AbstractValidator<CreateExamRequest>
{
    public CreateExamRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.TotalMarks).GreaterThan(0);
        RuleFor(x => x.PassMark).GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(x => x.TotalMarks)
            .WithMessage("Pass mark cannot exceed total marks.");
    }
}

public class SubmitResultsRequestValidator : AbstractValidator<SubmitResultsRequest>
{
    public SubmitResultsRequestValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.Entries).NotEmpty();
        RuleForEach(x => x.Entries).ChildRules(entry =>
        {
            entry.RuleFor(e => e.StudentId).NotEmpty();
            entry.RuleFor(e => e.MarksObtained).GreaterThanOrEqualTo(0);
        });
    }
}
