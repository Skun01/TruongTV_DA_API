using Application.DTOs.Users;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Users;

public class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(BeValidRole)
            .WithMessage("Role is invalid");
    }

    private static bool BeValidRole(string? role)
    {
        return Enum.TryParse<UserRole>(role, true, out _);
    }
}
