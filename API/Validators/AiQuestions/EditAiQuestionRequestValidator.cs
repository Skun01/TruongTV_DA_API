using Application.DTOs.AiQuestions;
using FluentValidation;

namespace API.Validators.AiQuestions;

public class EditAiQuestionRequestValidator : AbstractValidator<EditAiQuestionRequest>
{
    public EditAiQuestionRequestValidator()
    {
        RuleFor(x => x.GeneratedData)
            .NotEmpty().WithMessage("GeneratedData is required.");
    }
}
