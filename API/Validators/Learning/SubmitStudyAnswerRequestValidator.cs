using Application.DTOs.Learning;
using FluentValidation;

namespace API.Validators.Learning;

public class SubmitStudyAnswerRequestValidator : AbstractValidator<SubmitStudyAnswerRequest>
{
    public SubmitStudyAnswerRequestValidator()
    {
        RuleFor(x => x.CardId)
            .NotEmpty()
            .MaximumLength(100);

        RuleForEach(x => x.Answers)
            .NotEmpty()
            .MaximumLength(200);

        RuleForEach(x => x.SelectedOptionIds)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.FlashcardResult)
            .Must(result => result is "Learning" or "Known")
            .When(x => !string.IsNullOrWhiteSpace(x.FlashcardResult));
    }
}
