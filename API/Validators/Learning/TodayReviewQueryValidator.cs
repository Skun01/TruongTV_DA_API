using Application.DTOs.Learning;
using FluentValidation;

namespace API.Validators.Learning;

public class TodayReviewQueryValidator : AbstractValidator<TodayReviewQuery>
{
    public TodayReviewQueryValidator()
    {
        RuleFor(x => x.DeckId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.DeckId));

        RuleForEach(x => x.FolderIds)
            .NotEmpty()
            .MaximumLength(100);
    }
}
