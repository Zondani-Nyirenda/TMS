using FluentValidation;
using TMS.Application.DTOs.StudyMaterial;

namespace TMS.Application.Validators;

public class CreateStudyMaterialValidator : AbstractValidator<CreateStudyMaterialRequest>
{
    private static readonly string[] AllowedMimeTypes =
    [
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/epub+zip",
        "video/mp4",
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    public CreateStudyMaterialValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(250).WithMessage("Title cannot exceed 250 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.FileBase64)
            .NotEmpty().WithMessage("File is required.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.");

        RuleFor(x => x.MimeType)
            .NotEmpty().WithMessage("File type is required.")
            .Must(m => AllowedMimeTypes.Contains(m))
            .WithMessage("File type is not supported.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("File size is invalid.")
            .LessThanOrEqualTo(500 * 1024 * 1024)
            .WithMessage("File size cannot exceed 500 MB.");

        RuleFor(x => x.AcademicYear)
            .InclusiveBetween(2000, DateTime.UtcNow.Year + 1)
            .WithMessage("Academic year is invalid.")
            .When(x => x.AcademicYear.HasValue);

        RuleFor(x => x.DurationSeconds)
            .GreaterThan(0).WithMessage("Duration must be greater than 0.")
            .When(x => x.DurationSeconds.HasValue);
    }
}