using Application.DTOs.Auth;
using Domain.Constants;
using FluentValidation;

namespace API.Validators.Auth;

public class UploadAvatarFormRequestValidator : AbstractValidator<UploadAvatarFormRequest>
{
    public UploadAvatarFormRequestValidator()
    {
        RuleFor(x => x.Avatar)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Avatar is required")
            .Must(file => file.Length > 0)
            .WithMessage("Avatar must not be empty")
            .Must(file => file.Length <= 5 * 1024 * 1024)
            .WithMessage("Avatar size must not exceed 5 MB")
            .Must(file => FileUploadConstants.AllowedAvatarMimeTypes.Contains(file.ContentType))
            .WithMessage("Avatar content type is not supported");
    }
}
