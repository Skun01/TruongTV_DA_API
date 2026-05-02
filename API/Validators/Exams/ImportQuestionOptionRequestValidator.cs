using Application.DTOs.Exams;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Exams;

public class ImportQuestionOptionRequestValidator : AbstractValidator<ImportQuestionOptionRequest>
{
    public ImportQuestionOptionRequestValidator()
    {
        RuleFor(x => x.Label)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<OptionLabel>(value.Trim(), true, out _))
            .WithMessage("Label is invalid.");

        RuleFor(x => x.OptionType)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<OptionType>(value.Trim(), true, out _))
            .WithMessage("OptionType is invalid.");

        RuleFor(x => x.Text)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Text));

        RuleFor(x => x.ImageUrl)
            .MaximumLength(512)
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
    }
}
