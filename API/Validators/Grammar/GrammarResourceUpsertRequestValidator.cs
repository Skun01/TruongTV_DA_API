using Application.DTOs.Grammar;
using FluentValidation;

namespace API.Validators.Grammar;

public class GrammarResourceUpsertRequestValidator : AbstractValidator<GrammarResourceUpsertRequest>
{
    public GrammarResourceUpsertRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(2000)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Url is invalid.");
    }
}
