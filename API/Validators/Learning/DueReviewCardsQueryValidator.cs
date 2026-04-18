using Application.DTOs.Learning;
using FluentValidation;

namespace API.Validators.Learning;

public class DueReviewCardsQueryValidator : AbstractValidator<DueReviewCardsQuery>
{
    public DueReviewCardsQueryValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .When(x => x.Limit.HasValue);
    }
}
