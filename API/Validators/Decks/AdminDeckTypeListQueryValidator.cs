using Application.DTOs.Decks;
using FluentValidation;

namespace API.Validators.Decks;

public class AdminDeckTypeListQueryValidator : AbstractValidator<AdminDeckTypeListQuery>
{
    public AdminDeckTypeListQueryValidator()
    {
        RuleFor(x => x.Q)
            .MaximumLength(100)
            .When(x => x.Q != null);
    }
}
