using Application.DTOs.Users;
using FluentValidation;

namespace API.Validators.Users;

public class UpdateUserVerificationRequestValidator : AbstractValidator<UpdateUserVerificationRequest>
{
    public UpdateUserVerificationRequestValidator()
    {
        RuleFor(x => x.IsVerified)
            .NotNull();
    }
}
