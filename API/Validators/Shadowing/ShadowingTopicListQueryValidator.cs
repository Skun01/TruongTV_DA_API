using Application.DTOs.Shadowing;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Shadowing;

public class ShadowingTopicListQueryValidator : AbstractValidator<ShadowingTopicListQuery>
{
    public ShadowingTopicListQueryValidator()
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
    }
}
