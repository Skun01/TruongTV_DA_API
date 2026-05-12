using Application.DTOs.Vocabulary;
using FluentValidation;

namespace API.Validators.Vocabulary;

public class VocabularySentenceUpsertRequestValidator : AbstractValidator<VocabularySentenceUpsertRequest>
{
    public VocabularySentenceUpsertRequestValidator()
    {
        RuleFor(x => x.Id)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Id));

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Position)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Meaning)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Level)
            .Must(level => level is "N5" or "N4" or "N3" or "N2" or "N1")
            .When(x => !string.IsNullOrWhiteSpace(x.Level));

        RuleFor(x => x.BlankWord)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.BlankWord));

        RuleFor(x => x.Hint)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Hint));

        RuleForEach(x => x.AnswerList)
            .NotEmpty()
            .MaximumLength(200);
    }
}
