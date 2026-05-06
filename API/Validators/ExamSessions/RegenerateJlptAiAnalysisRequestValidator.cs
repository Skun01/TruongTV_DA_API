using Application.DTOs.ExamSessions;
using FluentValidation;

namespace API.Validators.ExamSessions;

public class RegenerateJlptAiAnalysisRequestValidator : AbstractValidator<RegenerateJlptAiAnalysisRequest>
{
    public RegenerateJlptAiAnalysisRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(50);
    }
}
