using Application.DTOs.Vocabulary;
using FluentValidation;

namespace API.Validators.Vocabulary;

public class ImportVocabularyRequestValidator : AbstractValidator<ImportVocabularyRequest>
{
    public ImportVocabularyRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new ImportVocabularyItemRequestValidator());
    }
}
