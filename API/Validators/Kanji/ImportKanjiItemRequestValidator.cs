using Application.DTOs.Kanji;
using FluentValidation;

namespace API.Validators.Kanji;

public class ImportKanjiItemRequestValidator : AbstractValidator<ImportKanjiItemRequest>
{
    public ImportKanjiItemRequestValidator()
    {
        RuleFor(x => x.RowNumber)
            .GreaterThan(0)
            .When(x => x.RowNumber.HasValue);
    }
}
