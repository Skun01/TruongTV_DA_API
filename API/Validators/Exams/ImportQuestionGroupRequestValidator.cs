using Application.DTOs.Exams;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Exams;

public class ImportQuestionGroupRequestValidator : AbstractValidator<ImportQuestionGroupRequest>
{
    public ImportQuestionGroupRequestValidator()
    {
        RuleFor(x => x.PassageText)
            .MaximumLength(10000)
            .When(x => !string.IsNullOrWhiteSpace(x.PassageText));

        RuleFor(x => x.AudioUrl)
            .MaximumLength(512)
            .When(x => !string.IsNullOrWhiteSpace(x.AudioUrl));

        RuleFor(x => x.AudioScript)
            .MaximumLength(10000)
            .When(x => !string.IsNullOrWhiteSpace(x.AudioScript));

        RuleFor(x => x.Instruction)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Instruction));

        RuleFor(x => x.MondaiType)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<ChoukaiMondaiType>(value.Trim(), true, out _))
            .WithMessage("MondaiType is invalid.");

        RuleForEach(x => x.Questions)
            .SetValidator(new ImportExamQuestionRequestValidator())
            .When(x => x.Questions != null);
    }
}
