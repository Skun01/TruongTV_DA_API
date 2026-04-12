using Application.DTOs.Kanji;
using FluentValidation;

namespace API.Validators.Kanji;

public class KanjiRadicalUpsertRequestValidator : AbstractValidator<KanjiRadicalUpsertRequest>
{
    public KanjiRadicalUpsertRequestValidator()
    {
        RuleFor(x => x.Character)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.MeaningVi)
            .NotEmpty()
            .MaximumLength(500);
    }
}
