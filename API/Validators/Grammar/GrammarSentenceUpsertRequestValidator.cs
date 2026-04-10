using Application.DTOs.Grammar;
using FluentValidation;

namespace API.Validators.Grammar;

public class GrammarSentenceUpsertRequestValidator : AbstractValidator<GrammarSentenceUpsertRequest>
{
    public GrammarSentenceUpsertRequestValidator()
    {
        RuleFor(x => x.Id)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Id));

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Meaning)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.SpeakerId)
            .GreaterThan(0)
            .When(x => x.SpeakerId.HasValue);

        RuleFor(x => x.Level)
            .Must(level => level is "N5" or "N4" or "N3" or "N2" or "N1")
            .When(x => !string.IsNullOrWhiteSpace(x.Level));
    }
}
