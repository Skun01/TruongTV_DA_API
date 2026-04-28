using Application.DTOs.ShadowingAdmin;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.ShadowingAdmin;

public class UpdateShadowingTopicRequestValidator : AbstractValidator<UpdateShadowingTopicRequest>
{
    public UpdateShadowingTopicRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => x.Title != null);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description != null);

        RuleFor(x => x.CoverImageUrl)
            .MaximumLength(512)
            .When(x => x.CoverImageUrl != null);

        RuleFor(x => x.Level)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<JlptLevel>(value.Trim(), true, out _))
            .WithMessage("Level is invalid.");

        RuleFor(x => x.Visibility)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<DeckVisibility>(value.Trim(), true, out _))
            .WithMessage("Visibility is invalid.");

        RuleFor(x => x.Status)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<PublishStatus>(value.Trim(), true, out _))
            .WithMessage("Status is invalid.");
    }
}
