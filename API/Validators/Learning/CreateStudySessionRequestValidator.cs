using Application.DTOs.Learning;
using FluentValidation;

namespace API.Validators.Learning;

public class CreateStudySessionRequestValidator : AbstractValidator<CreateStudySessionRequest>
{
    public CreateStudySessionRequestValidator()
    {
        RuleFor(x => x.DeckId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Mode)
            .NotEmpty()
            .Must(mode => mode is "FillInBlank" or "MultipleChoice" or "Flashcard");

        RuleForEach(x => x.FolderIds)
            .NotEmpty()
            .MaximumLength(100);
    }
}
