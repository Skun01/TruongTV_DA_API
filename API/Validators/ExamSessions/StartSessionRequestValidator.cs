using Application.DTOs.ExamSessions;
using FluentValidation;

namespace API.Validators.ExamSessions;

public class StartSessionRequestValidator : AbstractValidator<StartSessionRequest>
{
    public StartSessionRequestValidator()
    {
        RuleFor(x => x.ExamId)
            .NotEmpty().WithMessage("ExamId is required.");
    }
}
