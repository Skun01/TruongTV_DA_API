using Application.DTOs.Cards;
using FluentValidation;

namespace API.Validators.Cards;

public class ExplainCardRequestValidator : AbstractValidator<ExplainCardRequest>
{
    public ExplainCardRequestValidator()
    {
        RuleFor(x => x.UserQuestion)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.UserQuestion));
    }
}
