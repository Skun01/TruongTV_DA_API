using Application.DTOs.Grammar;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Grammar;

public class UpdateGrammarCardRequestValidator : AbstractValidator<UpdateGrammarCardRequest>
{
    public UpdateGrammarCardRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Summary)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.Level)
            .MaximumLength(10);

        RuleFor(x => x.Level)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<JlptLevel>(value.Trim(), true, out _))
            .WithMessage("Level is invalid.");

        RuleFor(x => x.Status)
            .MaximumLength(20);

        RuleFor(x => x.Status)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<PublishStatus>(value.Trim(), true, out _))
            .WithMessage("Status is invalid.");

        RuleFor(x => x.Register)
            .MaximumLength(50);

        RuleFor(x => x.Register)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<RegisterType>(value.Trim(), true, out _))
            .WithMessage("Register is invalid.");

        RuleFor(x => x.Tags)
            .Must(tags => tags.Count <= 20)
            .WithMessage("Tags cannot exceed 20 items.");

        RuleForEach(x => x.Tags)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Structures)
            .NotEmpty()
            .Must(items => items.Count <= 30)
            .WithMessage("Structures cannot exceed 30 items.");

        RuleForEach(x => x.Structures)
            .SetValidator(new GrammarStructureRequestValidator());

        RuleFor(x => x.Explanation)
            .MaximumLength(10000);

        RuleFor(x => x.Caution)
            .MaximumLength(5000);

        RuleForEach(x => x.AlternateForms)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.Relations)
            .Must(items => items.Count <= 50)
            .WithMessage("Relations cannot exceed 50 items.");

        RuleForEach(x => x.Relations)
            .SetValidator(new GrammarRelationUpsertRequestValidator());

        RuleFor(x => x.Resources)
            .Must(items => items.Count <= 30)
            .WithMessage("Resources cannot exceed 30 items.");

        RuleForEach(x => x.Resources)
            .SetValidator(new GrammarResourceUpsertRequestValidator());

        RuleFor(x => x.Sentences)
            .Must(items => items.Count <= 20)
            .WithMessage("Sentences cannot exceed 20 items.");

        RuleForEach(x => x.Sentences)
            .SetValidator(new GrammarSentenceUpsertRequestValidator());
    }
}
