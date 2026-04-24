using Application.DTOs.ShadowingAdmin;
using FluentValidation;

namespace API.Validators.ShadowingAdmin;

public class AttachShadowingTopicSentenceRequestValidator : AbstractValidator<AttachShadowingTopicSentenceRequest>
{
    public AttachShadowingTopicSentenceRequestValidator()
    {
        RuleFor(x => x.SentenceId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Position)
            .GreaterThan(0);

        RuleFor(x => x.Note)
            .MaximumLength(1000)
            .When(x => x.Note != null);
    }
}
