using Application.DTOs.Grammar;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Grammar;

public class GrammarExportQueryValidator : AbstractValidator<GrammarExportQuery>
{
    public GrammarExportQueryValidator()
    {
        RuleFor(x => x.Level)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<JlptLevel>(value.Trim(), true, out _))
            .WithMessage("Level is invalid.");

        RuleFor(x => x.Status)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<PublishStatus>(value.Trim(), true, out _))
            .WithMessage("Status is invalid.");

        RuleFor(x => x.Register)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<RegisterType>(value.Trim(), true, out _))
            .WithMessage("Register is invalid.");
    }
}
