using Application.DTOs.Decks;
using FluentValidation;

namespace API.Validators.Decks;

public class UpdateDeckTypeRequestValidator : AbstractValidator<UpdateDeckTypeRequest>
{
    public UpdateDeckTypeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
