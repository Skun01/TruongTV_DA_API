using Application.DTOs.Voicevox;
using FluentValidation;

namespace API.Validators.Voicevox;

public class VoicevoxPreviewRequestValidator : AbstractValidator<VoicevoxPreviewRequest>
{
    public VoicevoxPreviewRequestValidator()
    {
        RuleFor(x => x.SpeakerId)
            .GreaterThan(0);

        RuleFor(x => x.Text)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Text));
    }
}
