using Application.DTOs.ShadowingAdmin;
using FluentValidation;

namespace API.Validators.ShadowingAdmin;

public class ReorderShadowingTopicSentencesRequestValidator : AbstractValidator<ReorderShadowingTopicSentencesRequest>
{
    public ReorderShadowingTopicSentencesRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotNull()
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new ReorderShadowingTopicSentenceItemRequestValidator());
    }
}
