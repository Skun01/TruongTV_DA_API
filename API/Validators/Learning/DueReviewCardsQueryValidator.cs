using Application.DTOs.Learning;
using FluentValidation;

namespace API.Validators.Learning;

public class DueReviewCardsQueryValidator : AbstractValidator<DueReviewCardsQuery>
{
    public DueReviewCardsQueryValidator()
    {
    }
}
