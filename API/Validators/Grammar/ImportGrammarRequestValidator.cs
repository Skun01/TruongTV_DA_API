using Application.DTOs.Grammar;
using FluentValidation;

namespace API.Validators.Grammar;

public class ImportGrammarRequestValidator : AbstractValidator<ImportGrammarRequest>
{
    public ImportGrammarRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new ImportGrammarItemRequestValidator());
    }
}
