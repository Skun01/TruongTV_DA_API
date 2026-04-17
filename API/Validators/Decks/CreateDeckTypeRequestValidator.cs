using Application.DTOs.Decks;
using FluentValidation;

namespace API.Validators.Decks;

public class CreateDeckTypeRequestValidator : AbstractValidator<CreateDeckTypeRequest>
{
    public CreateDeckTypeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
