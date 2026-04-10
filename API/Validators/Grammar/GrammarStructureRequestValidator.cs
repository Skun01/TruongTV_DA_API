using Application.DTOs.Grammar;
using FluentValidation;

namespace API.Validators.Grammar;

public class GrammarStructureRequestValidator : AbstractValidator<GrammarStructureRequest>
{
    public GrammarStructureRequestValidator()
    {
        RuleFor(x => x.Pattern)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.Annotations)
            .Must(annotations => annotations == null || annotations.Count <= 20)
            .WithMessage("Annotations cannot exceed 20 items.");

        RuleFor(x => x.Annotations)
            .Must(annotations => annotations == null || annotations.Keys.All(key =>
                !string.IsNullOrWhiteSpace(key)
                && key.Trim().Length <= 20
                && key.All(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '-')))
            .WithMessage("Annotation key is invalid.");

        RuleFor(x => x.Annotations)
            .Must(annotations => annotations == null || annotations.Values.All(value =>
                !string.IsNullOrWhiteSpace(value)
                && value.Trim().Length <= 1000))
            .WithMessage("Annotation value is invalid.");
    }
}
