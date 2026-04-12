using Application.DTOs.Kanji;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Kanji;

public class UpdateKanjiCardRequestValidator : AbstractValidator<UpdateKanjiCardRequest>
{
    public UpdateKanjiCardRequestValidator()
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

        RuleFor(x => x.Tags)
            .Must(tags => tags.Count <= 20)
            .WithMessage("Tags cannot exceed 20 items.");

        RuleForEach(x => x.Tags)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Kanji)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.StrokeCount)
            .GreaterThan(0);

        RuleFor(x => x.StrokeOrderUrl)
            .MaximumLength(2000);

        RuleFor(x => x.HanViet)
            .MaximumLength(200);

        RuleFor(x => x.MeaningVi)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.Onyomi)
            .Must(items => items.Count <= 20)
            .WithMessage("Onyomi cannot exceed 20 items.");

        RuleForEach(x => x.Onyomi)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Kunyomi)
            .Must(items => items.Count <= 20)
            .WithMessage("Kunyomi cannot exceed 20 items.");

        RuleForEach(x => x.Kunyomi)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Radicals)
            .NotEmpty()
            .Must(items => items.Count <= 30)
            .WithMessage("Radicals cannot exceed 30 items.");

        RuleForEach(x => x.Radicals)
            .SetValidator(new KanjiRadicalUpsertRequestValidator());
    }
}
