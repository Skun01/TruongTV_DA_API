using Application.DTOs.ShadowingAdmin;
using FluentValidation;

namespace API.Validators.ShadowingAdmin;

public class ReorderShadowingTopicSentenceItemRequestValidator : AbstractValidator<ReorderShadowingTopicSentenceItemRequest>
{
    public ReorderShadowingTopicSentenceItemRequestValidator()
    {
        RuleFor(x => x.SentenceId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Position)
            .GreaterThan(0);
    }
}
