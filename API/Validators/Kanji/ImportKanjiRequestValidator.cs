using Application.DTOs.Kanji;
using FluentValidation;

namespace API.Validators.Kanji;

public class ImportKanjiRequestValidator : AbstractValidator<ImportKanjiRequest>
{
    public ImportKanjiRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new ImportKanjiItemRequestValidator());
    }
}
