using Application.DTOs.Vocabulary;
using FluentValidation;

namespace API.Validators.Vocabulary;

public class ImportVocabularyItemRequestValidator : AbstractValidator<ImportVocabularyItemRequest>
{
    public ImportVocabularyItemRequestValidator()
    {
        RuleFor(x => x.RowNumber)
            .GreaterThan(0)
            .When(x => x.RowNumber.HasValue);
    }
}
