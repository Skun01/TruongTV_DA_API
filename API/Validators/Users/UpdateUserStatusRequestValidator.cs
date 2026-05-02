using Application.DTOs.Users;
using FluentValidation;

namespace API.Validators.Users;

public class UpdateUserStatusRequestValidator : AbstractValidator<UpdateUserStatusRequest>
{
    public UpdateUserStatusRequestValidator()
    {
        RuleFor(x => x.IsActive)
            .NotNull();
    }
}
