using Application.DTOs.Users;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Users;

public class AdminUserListQueryValidator : AbstractValidator<AdminUserListQuery>
{
    public AdminUserListQueryValidator()
    {
        RuleFor(x => x.Q)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Q));

        RuleFor(x => x.Role)
            .Must(BeValidRole)
            .When(x => !string.IsNullOrWhiteSpace(x.Role))
            .WithMessage("Role is invalid");
    }

    private static bool BeValidRole(string? role)
    {
        return Enum.TryParse<UserRole>(role, true, out _);
    }
}
