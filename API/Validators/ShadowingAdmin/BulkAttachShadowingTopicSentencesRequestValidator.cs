using Application.DTOs.ShadowingAdmin;
using FluentValidation;

namespace API.Validators.ShadowingAdmin;

public class BulkAttachShadowingTopicSentencesRequestValidator : AbstractValidator<BulkAttachShadowingTopicSentencesRequest>
{
    public BulkAttachShadowingTopicSentencesRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotNull()
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new AttachShadowingTopicSentenceRequestValidator());
    }
}
