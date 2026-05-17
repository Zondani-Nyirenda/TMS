using FluentValidation;
using TMS.Application.DTOs.StudyMaterial;

namespace TMS.Application.Validators;

public class CreateMaterialCategoryValidator : AbstractValidator<CreateMaterialCategoryRequest>
{
    public CreateMaterialCategoryValidator()
    {
        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Subject is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(300).WithMessage("Description cannot exceed 300 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be 0 or greater.");
    }
}