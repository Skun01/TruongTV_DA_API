using Application.DTOs.Shadowing;
using Domain.Constants;
using FluentValidation;

namespace API.Validators.Shadowing;

public class SubmitShadowingAttemptFormRequestValidator : AbstractValidator<SubmitShadowingAttemptFormRequest>
{
    public SubmitShadowingAttemptFormRequestValidator()
    {
        RuleFor(x => x.TopicId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.SentenceId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Locale)
            .MaximumLength(20)
            .When(x => x.Locale != null);

        RuleFor(x => x.Audio)
            .NotNull()
            .Must(file => file.Length > 0)
            .WithMessage("Audio must not be empty")
            .Must(file => file.Length <= 20 * 1024 * 1024)
            .WithMessage("Audio size must not exceed 20 MB")
            .Must(file => FileUploadConstants.AllowedAudioMimeTypes.Contains(file.ContentType))
            .WithMessage("Audio content type is not supported");
    }
}
