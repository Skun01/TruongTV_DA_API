using Application.DTOs.Decks;
using FluentValidation;

namespace API.Validators.Decks;

public class ReorderDeckFoldersRequestValidator : AbstractValidator<ReorderDeckFoldersRequest>
{
    public ReorderDeckFoldersRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new ReorderDeckFolderItemRequestValidator());
    }
}
