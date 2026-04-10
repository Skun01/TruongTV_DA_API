using Application.DTOs.Sentences;
using FluentValidation;

namespace API.Validators.Sentences;

public class ImportSentenceItemRequestValidator : AbstractValidator<ImportSentenceItemRequest>
{
    public ImportSentenceItemRequestValidator()
    {
        RuleFor(x => x.RowNumber)
            .GreaterThan(0)
            .When(x => x.RowNumber.HasValue);

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
