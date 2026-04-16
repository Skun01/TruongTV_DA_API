using Application.DTOs.Decks;
using FluentValidation;

namespace API.Validators.Decks;

public class BookmarkedDeckListQueryValidator : AbstractValidator<BookmarkedDeckListQuery>
{
    public BookmarkedDeckListQueryValidator()
    {
        RuleFor(x => x.Q)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Q));

        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
