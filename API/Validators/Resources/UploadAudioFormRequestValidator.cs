using Application.DTOs.Resources;
using Domain.Constants;
using FluentValidation;

namespace API.Validators.Resources;

public class UploadAudioFormRequestValidator : AbstractValidator<UploadAudioFormRequest>
{
    public UploadAudioFormRequestValidator()
    {
        RuleFor(x => x.Audio)
            .NotNull()
            .WithMessage("Audio is required")
            .Must(file => file.Length > 0)
            .WithMessage("Audio must not be empty")
            .Must(file => file.Length <= 20 * 1024 * 1024)
            .WithMessage("Audio size must not exceed 20 MB")
            .Must(file => FileUploadConstants.AllowedAudioMimeTypes.Contains(file.ContentType))
            .WithMessage("Audio content type is not supported");
    }
}
