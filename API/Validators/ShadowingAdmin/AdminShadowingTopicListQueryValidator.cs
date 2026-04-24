using Application.DTOs.ShadowingAdmin;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.ShadowingAdmin;

public class AdminShadowingTopicListQueryValidator : AbstractValidator<AdminShadowingTopicListQuery>
{
    public AdminShadowingTopicListQueryValidator()
    {
        RuleFor(x => x.Q)
            .MaximumLength(200)
            .When(x => x.Q != null);

        RuleFor(x => x.Level)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<JlptLevel>(value.Trim(), true, out _))
            .WithMessage("Level is invalid.");

        RuleFor(x => x.Visibility)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<DeckVisibility>(value.Trim(), true, out _))
            .WithMessage("Visibility is invalid.");

        RuleFor(x => x.Status)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<PublishStatus>(value.Trim(), true, out _))
            .WithMessage("Status is invalid.");

        RuleFor(x => x.CreatedBy)
            .MaximumLength(50)
            .When(x => x.CreatedBy != null);
    }
}
