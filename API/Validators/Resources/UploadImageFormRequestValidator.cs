using Application.DTOs.Resources;
using Domain.Constants;
using FluentValidation;

namespace API.Validators.Resources;

public class UploadImageFormRequestValidator : AbstractValidator<UploadImageFormRequest>
{
    public UploadImageFormRequestValidator()
    {
        RuleFor(x => x.Image)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Image is required")
            .Must(file => file.Length > 0)
            .WithMessage("Image must not be empty")
            .Must(file => file.Length <= 10 * 1024 * 1024)
            .WithMessage("Image size must not exceed 10 MB")
            .Must(file => FileUploadConstants.AllowedImageMimeTypes.Contains(file.ContentType))
            .WithMessage("Image content type is not supported");
    }
}
