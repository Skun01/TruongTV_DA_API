using Application.DTOs.Decks;
using FluentValidation;

namespace API.Validators.Decks;

public class ToggleDeckBookmarkRequestValidator : AbstractValidator<ToggleDeckBookmarkRequest>
{
    public ToggleDeckBookmarkRequestValidator()
    {
        RuleFor(x => x.Bookmarked)
            .NotNull();
    }
}
