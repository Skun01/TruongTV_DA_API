using Application.DTOs.Decks;
using FluentValidation;

namespace API.Validators.Decks;

public class AddCardToFolderRequestValidator : AbstractValidator<AddCardToFolderRequest>
{
    public AddCardToFolderRequestValidator()
    {
        RuleFor(x => x.CardId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Position)
            .GreaterThan(0)
            .When(x => x.Position.HasValue);
    }
}
