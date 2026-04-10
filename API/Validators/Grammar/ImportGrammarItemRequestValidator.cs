using Application.DTOs.Grammar;
using FluentValidation;

namespace API.Validators.Grammar;

public class ImportGrammarItemRequestValidator : AbstractValidator<ImportGrammarItemRequest>
{
    public ImportGrammarItemRequestValidator()
    {
        RuleFor(x => x.RowNumber)
            .GreaterThan(0)
            .When(x => x.RowNumber.HasValue);
    }
}
