using Application.DTOs.ExamSessions;
using FluentValidation;

namespace API.Validators.ExamSessions;

public class SaveAnswerRequestValidator : AbstractValidator<SaveAnswerRequest>
{
    public SaveAnswerRequestValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("QuestionId is required.");
    }
}
