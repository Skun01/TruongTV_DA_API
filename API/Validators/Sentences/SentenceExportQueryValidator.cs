using Application.DTOs.Sentences;
using FluentValidation;

namespace API.Validators.Sentences;

public class SentenceExportQueryValidator : AbstractValidator<SentenceExportQuery>
{
    public SentenceExportQueryValidator()
    {
        RuleFor(x => x.Level)
            .Must(level => level is "N5" or "N4" or "N3" or "N2" or "N1")
            .When(x => !string.IsNullOrWhiteSpace(x.Level));
    }
}
