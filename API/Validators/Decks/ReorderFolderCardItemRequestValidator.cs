using Application.DTOs.Decks;
using FluentValidation;

namespace API.Validators.Decks;

public class ReorderFolderCardItemRequestValidator : AbstractValidator<ReorderFolderCardItemRequest>
{
    public ReorderFolderCardItemRequestValidator()
    {
        RuleFor(x => x.CardId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Position)
            .GreaterThan(0);
    }
}
