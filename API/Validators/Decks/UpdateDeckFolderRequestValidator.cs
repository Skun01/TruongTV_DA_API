using Application.DTOs.Decks;
using FluentValidation;

namespace API.Validators.Decks;

public class UpdateDeckFolderRequestValidator : AbstractValidator<UpdateDeckFolderRequest>
{
    public UpdateDeckFolderRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description != null);
    }
}
