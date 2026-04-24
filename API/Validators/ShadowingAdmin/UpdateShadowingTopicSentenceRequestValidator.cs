using Application.DTOs.ShadowingAdmin;
using FluentValidation;

namespace API.Validators.ShadowingAdmin;

public class UpdateShadowingTopicSentenceRequestValidator : AbstractValidator<UpdateShadowingTopicSentenceRequest>
{
    public UpdateShadowingTopicSentenceRequestValidator()
    {
        RuleFor(x => x.Position)
            .GreaterThan(0);

        RuleFor(x => x.Note)
            .MaximumLength(1000)
            .When(x => x.Note != null);
    }
}
