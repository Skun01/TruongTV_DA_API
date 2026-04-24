using Application.DTOs.Shadowing;
using FluentValidation;

namespace API.Validators.Shadowing;

public class ShadowingAttemptHistoryQueryValidator : AbstractValidator<ShadowingAttemptHistoryQuery>
{
    public ShadowingAttemptHistoryQueryValidator()
    {
        RuleFor(x => x.TopicId)
            .MaximumLength(50)
            .When(x => x.TopicId != null);

        RuleFor(x => x.SentenceId)
            .MaximumLength(50)
            .When(x => x.SentenceId != null);
    }
}
