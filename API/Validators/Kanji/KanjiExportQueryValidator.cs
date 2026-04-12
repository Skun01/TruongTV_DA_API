using Application.DTOs.Kanji;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Kanji;

public class KanjiExportQueryValidator : AbstractValidator<KanjiExportQuery>
{
    public KanjiExportQueryValidator()
    {
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

        RuleFor(x => x.StrokeCountMin)
            .GreaterThan(0)
            .When(x => x.StrokeCountMin.HasValue);

        RuleFor(x => x.StrokeCountMax)
            .GreaterThan(0)
            .When(x => x.StrokeCountMax.HasValue);

        RuleFor(x => x)
            .Must(x => !x.StrokeCountMin.HasValue || !x.StrokeCountMax.HasValue || x.StrokeCountMin.Value <= x.StrokeCountMax.Value)
            .WithMessage("Stroke count range is invalid.");

        RuleFor(x => x.Radical)
            .MaximumLength(20);
    }
}
