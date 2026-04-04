using Application.DTOs.Sentences;
using FluentValidation;

namespace API.Validators.Sentences;

public class CreateSentenceRequestValidator : AbstractValidator<CreateSentenceRequest>
{
    public CreateSentenceRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Meaning)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.AudioUrl)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.AudioUrl));

        RuleFor(x => x.Level)
            .Must(level => level is "N5" or "N4" or "N3" or "N2" or "N1")
            .When(x => !string.IsNullOrWhiteSpace(x.Level));
    }
}
