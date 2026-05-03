using Application.DTOs.Learning;
using FluentValidation;

namespace API.Validators.Learning;

public class ExamHistoryQueryValidator : AbstractValidator<ExamHistoryQuery>
{
    public ExamHistoryQueryValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}
