using Application.DTOs.Decks;
using FluentValidation;

namespace API.Validators.Decks;

public class ReorderDeckFolderItemRequestValidator : AbstractValidator<ReorderDeckFolderItemRequest>
{
    public ReorderDeckFolderItemRequestValidator()
    {
        RuleFor(x => x.FolderId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Position)
            .GreaterThan(0);
    }
}
