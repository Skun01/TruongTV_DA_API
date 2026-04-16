using Application.DTOs.Decks;
using FluentValidation;

namespace API.Validators.Decks;

public class CreateDeckFolderRequestValidator : AbstractValidator<CreateDeckFolderRequest>
{
    public CreateDeckFolderRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description != null);

        RuleFor(x => x.Position)
            .GreaterThan(0)
            .When(x => x.Position.HasValue);
    }
}
