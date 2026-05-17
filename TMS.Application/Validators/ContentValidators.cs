using FluentValidation;
using TMS.Application.DTOs.Content;
using TMS.Domain.Enums;

namespace TMS.Application.Validators;

public class CreateContentModuleRequestValidator : AbstractValidator<CreateContentModuleRequest>
{
    public CreateContentModuleRequestValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateContentModuleRequestValidator : AbstractValidator<UpdateContentModuleRequest>
{
    public UpdateContentModuleRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateContentItemRequestValidator : AbstractValidator<CreateContentItemRequest>
{
    public CreateContentItemRequestValidator()
    {
        RuleFor(x => x.ContentModuleId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);

        // Link type must supply ExternalUrl; file types must not
        When(x => x.ResourceType == ResourceType.Link, () =>
            RuleFor(x => x.ExternalUrl).NotEmpty()
                .WithMessage("ExternalUrl is required for Link resources.")
                .MaximumLength(2000)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("ExternalUrl must be a valid absolute URL."));

        When(x => x.ResourceType != ResourceType.Link, () =>
            RuleFor(x => x.ExternalUrl).Empty()
                .WithMessage("ExternalUrl should only be set for Link resources."));
    }
}

public class UpdateContentItemRequestValidator : AbstractValidator<UpdateContentItemRequest>
{
    public UpdateContentItemRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class GrantAccessRequestValidator : AbstractValidator<GrantAccessRequest>
{
    public GrantAccessRequestValidator()
    {
        RuleFor(x => x.ContentItemId).NotEmpty();
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("ExpiresAt must be in the future.")
            .When(x => x.ExpiresAt.HasValue);
    }
}