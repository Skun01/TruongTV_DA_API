using Application.DTOs.Learning;
using FluentValidation;

namespace API.Validators.Learning;

public class StudySessionSettingsRequestValidator : AbstractValidator<StudySessionSettingsRequest>
{
    public StudySessionSettingsRequestValidator()
    {
        RuleFor(x => x.FlashcardFront)
            .Must(value => value is "Title" or "Summary")
            .When(x => !string.IsNullOrWhiteSpace(x.FlashcardFront));

        RuleFor(x => x.FlashcardBack)
            .Must(value => value is "Title" or "Summary")
            .When(x => !string.IsNullOrWhiteSpace(x.FlashcardBack));

        RuleFor(x => x.MultipleChoiceQuestion)
            .Must(value => value is "TitleToSummary" or "SummaryToTitle")
            .When(x => !string.IsNullOrWhiteSpace(x.MultipleChoiceQuestion));
    }
}
