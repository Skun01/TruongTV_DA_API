using Application.DTOs.Auth;
using FluentValidation;

namespace API.Validators.Auth;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50);

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(512)
            .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl));
    }
}