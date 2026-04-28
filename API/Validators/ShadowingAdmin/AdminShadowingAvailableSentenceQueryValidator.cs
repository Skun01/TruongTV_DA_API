using Application.DTOs.ShadowingAdmin;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.ShadowingAdmin;

public class AdminShadowingAvailableSentenceQueryValidator : AbstractValidator<AdminShadowingAvailableSentenceQuery>
{
    public AdminShadowingAvailableSentenceQueryValidator()
    {
        RuleFor(x => x.Q)
            .MaximumLength(200)
            .When(x => x.Q != null);

        RuleFor(x => x.Level)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<JlptLevel>(value.Trim(), true, out _))
            .WithMessage("Level is invalid.");
    }
}
