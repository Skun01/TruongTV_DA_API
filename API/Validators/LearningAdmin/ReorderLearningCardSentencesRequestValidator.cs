using Application.DTOs.LearningAdmin;
using FluentValidation;

namespace API.Validators.LearningAdmin;

public class ReorderLearningCardSentencesRequestValidator : AbstractValidator<ReorderLearningCardSentencesRequest>
{
    public ReorderLearningCardSentencesRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new ReorderLearningCardSentenceItemRequestValidator());

        RuleFor(x => x.Items)
            .Must(items => items
                .Where(i => !string.IsNullOrWhiteSpace(i.SentenceId))
                .Select(i => i.SentenceId.Trim())
                .Distinct(StringComparer.Ordinal)
                .Count() == items.Count)
            .WithMessage("SentenceId values must be unique.");

        RuleFor(x => x.Items)
            .Must(items => items
                .Select(i => i.Position)
                .Distinct()
                .Count() == items.Count)
            .WithMessage("Sentence positions must be unique.");
    }
}
