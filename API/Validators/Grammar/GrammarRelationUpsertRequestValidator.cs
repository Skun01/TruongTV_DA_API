using Application.DTOs.Grammar;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Grammar;

public class GrammarRelationUpsertRequestValidator : AbstractValidator<GrammarRelationUpsertRequest>
{
    public GrammarRelationUpsertRequestValidator()
    {
        RuleFor(x => x.RelatedId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.RelationType)
            .NotEmpty()
            .Must(value => Enum.TryParse<GrammarRelationType>(value.Trim(), true, out _))
            .WithMessage("RelationType is invalid.");
    }
}
