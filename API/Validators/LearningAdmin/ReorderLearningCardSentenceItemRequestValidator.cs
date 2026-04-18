using Application.DTOs.LearningAdmin;
using FluentValidation;

namespace API.Validators.LearningAdmin;

public class ReorderLearningCardSentenceItemRequestValidator : AbstractValidator<ReorderLearningCardSentenceItemRequest>
{
    public ReorderLearningCardSentenceItemRequestValidator()
    {
        RuleFor(x => x.SentenceId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Position)
            .GreaterThan(0);
    }
}
