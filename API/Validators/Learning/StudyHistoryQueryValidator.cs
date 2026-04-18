using Application.DTOs.Learning;
using FluentValidation;

namespace API.Validators.Learning;

public class StudyHistoryQueryValidator : AbstractValidator<StudyHistoryQuery>
{
    public StudyHistoryQueryValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .When(x => x.Limit.HasValue);
    }
}
