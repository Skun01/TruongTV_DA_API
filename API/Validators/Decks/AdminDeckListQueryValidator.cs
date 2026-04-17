using Application.DTOs.Decks;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Decks;

public class AdminDeckListQueryValidator : AbstractValidator<AdminDeckListQuery>
{
    public AdminDeckListQueryValidator()
    {
        RuleFor(x => x.Q)
            .MaximumLength(200)
            .When(x => x.Q != null);

        RuleFor(x => x.TypeId)
            .MaximumLength(50)
            .When(x => x.TypeId != null);

        RuleFor(x => x.CreatedBy)
            .MaximumLength(50)
            .When(x => x.CreatedBy != null);

        RuleFor(x => x.Status)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<PublishStatus>(value.Trim(), true, out _))
            .WithMessage("Status is invalid.");

        RuleFor(x => x.Visibility)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<DeckVisibility>(value.Trim(), true, out _))
            .WithMessage("Visibility is invalid.");
    }
}
