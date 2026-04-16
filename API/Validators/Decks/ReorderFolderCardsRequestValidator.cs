using Application.DTOs.Decks;
using FluentValidation;

namespace API.Validators.Decks;

public class ReorderFolderCardsRequestValidator : AbstractValidator<ReorderFolderCardsRequest>
{
    public ReorderFolderCardsRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new ReorderFolderCardItemRequestValidator());
    }
}
