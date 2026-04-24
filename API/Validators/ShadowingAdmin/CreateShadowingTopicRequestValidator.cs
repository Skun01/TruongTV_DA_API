using Application.DTOs.ShadowingAdmin;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.ShadowingAdmin;

public class CreateShadowingTopicRequestValidator : AbstractValidator<CreateShadowingTopicRequest>
{
    public CreateShadowingTopicRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);

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
