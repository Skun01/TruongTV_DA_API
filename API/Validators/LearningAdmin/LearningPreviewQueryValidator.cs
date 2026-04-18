using Application.DTOs.LearningAdmin;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.LearningAdmin;

public class LearningPreviewQueryValidator : AbstractValidator<LearningPreviewQuery>
{
    public LearningPreviewQueryValidator()
    {
        RuleFor(x => x.Mode)
            .NotEmpty()
            .Must(value => Enum.TryParse<StudyMode>(value.Trim(), true, out _))
            .WithMessage("Mode is invalid.");

        RuleFor(x => x.MultipleChoiceQuestion)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<MultipleChoiceQuestionType>(value.Trim(), true, out _))
            .WithMessage("MultipleChoiceQuestion is invalid.");

        RuleFor(x => x.FlashcardFront)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<FlashcardContentType>(value.Trim(), true, out _))
            .WithMessage("FlashcardFront is invalid.");

        RuleFor(x => x.FlashcardBack)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<FlashcardContentType>(value.Trim(), true, out _))
            .WithMessage("FlashcardBack is invalid.");
    }
}
