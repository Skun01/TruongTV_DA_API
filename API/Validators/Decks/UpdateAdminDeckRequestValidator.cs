using Application.DTOs.Decks;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Decks;

public class UpdateAdminDeckRequestValidator : AbstractValidator<UpdateAdminDeckRequest>
{
    public UpdateAdminDeckRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => x.Title != null);

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => x.Description != null);

        RuleFor(x => x.CoverImageUrl)
            .MaximumLength(2000)
            .When(x => x.CoverImageUrl != null);

        RuleFor(x => x.Visibility)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<DeckVisibility>(value.Trim(), true, out _))
            .WithMessage("Visibility is invalid.");

        RuleFor(x => x.Status)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<PublishStatus>(value.Trim(), true, out _))
            .WithMessage("Status is invalid.");

        RuleFor(x => x.TypeId)
            .MaximumLength(50)
            .When(x => x.TypeId != null);
    }
}
