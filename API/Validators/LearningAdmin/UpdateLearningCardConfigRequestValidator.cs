using Application.DTOs.LearningAdmin;
using FluentValidation;

namespace API.Validators.LearningAdmin;

public class UpdateLearningCardConfigRequestValidator : AbstractValidator<UpdateLearningCardConfigRequest>
{
    public UpdateLearningCardConfigRequestValidator()
    {
        RuleFor(x => x.Summary)
            .NotEmpty()
            .MaximumLength(1000);

        RuleForEach(x => x.Sentences)
            .SetValidator(new UpsertLearningCardSentenceConfigRequestValidator());

        RuleFor(x => x.Sentences)
            .Must(sentences => sentences
                .Where(s => !string.IsNullOrWhiteSpace(s.SentenceId))
                .Select(s => s.SentenceId.Trim())
                .Distinct(StringComparer.Ordinal)
                .Count() == sentences.Count)
            .WithMessage("SentenceId values must be unique.");

        RuleFor(x => x.Sentences)
            .Must(sentences => sentences
                .Select(s => s.Position)
                .Distinct()
                .Count() == sentences.Count)
            .WithMessage("Sentence positions must be unique.");
    }
}
