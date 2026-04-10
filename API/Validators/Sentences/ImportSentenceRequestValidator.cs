using Application.DTOs.Sentences;
using FluentValidation;

namespace API.Validators.Sentences;

public class ImportSentenceRequestValidator : AbstractValidator<ImportSentenceRequest>
{
    public ImportSentenceRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new ImportSentenceItemRequestValidator());
    }
}
