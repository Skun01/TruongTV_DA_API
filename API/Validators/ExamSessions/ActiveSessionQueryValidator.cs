using Application.DTOs.ExamSessions;
using FluentValidation;

namespace API.Validators.ExamSessions;

public class ActiveSessionQueryValidator : AbstractValidator<ActiveSessionQuery>
{
    public ActiveSessionQueryValidator()
    {
        RuleFor(x => x.ExamId)
            .NotEmpty().WithMessage("ExamId is required.");
    }
}
