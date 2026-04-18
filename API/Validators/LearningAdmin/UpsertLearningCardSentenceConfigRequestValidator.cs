using Application.DTOs.LearningAdmin;
using FluentValidation;

namespace API.Validators.LearningAdmin;

public class UpsertLearningCardSentenceConfigRequestValidator : AbstractValidator<UpsertLearningCardSentenceConfigRequest>
{
    public UpsertLearningCardSentenceConfigRequestValidator()
    {
        RuleFor(x => x.SentenceId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Position)
            .GreaterThan(0);

        RuleFor(x => x.BlankWord)
            .MaximumLength(500)
            .When(x => x.BlankWord != null);

        RuleFor(x => x.Hint)
            .MaximumLength(1000)
            .When(x => x.Hint != null);
    }
}
