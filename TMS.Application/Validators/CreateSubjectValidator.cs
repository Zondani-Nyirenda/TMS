using FluentValidation;
using TMS.Application.DTOs.StudyMaterial;

namespace TMS.Application.Validators;

public class CreateSubjectValidator : AbstractValidator<CreateSubjectRequest>
{
    public CreateSubjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Subject name is required.")
            .MaximumLength(150).WithMessage("Subject name cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.ColorHex)
            .Matches("^#([A-Fa-f0-9]{6})$").WithMessage("Color must be a valid hex code e.g. #4F46E5.")
            .When(x => !string.IsNullOrWhiteSpace(x.ColorHex));

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be 0 or greater.");
    }
}

public class UpdateSubjectValidator : AbstractValidator<UpdateSubjectRequest>
{
    public UpdateSubjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Subject name is required.")
            .MaximumLength(150).WithMessage("Subject name cannot exceed 150 characters.");

        RuleFor(x => x.ColorHex)
            .Matches("^#([A-Fa-f0-9]{6})$").WithMessage("Color must be a valid hex code e.g. #4F46E5.")
            .When(x => !string.IsNullOrWhiteSpace(x.ColorHex));
    }
}