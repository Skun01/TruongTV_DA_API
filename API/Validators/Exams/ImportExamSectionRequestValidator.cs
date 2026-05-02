using Application.DTOs.Exams;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Exams;

public class ImportExamSectionRequestValidator : AbstractValidator<ImportExamSectionRequest>
{
    public ImportExamSectionRequestValidator()
    {
        RuleFor(x => x.SectionType)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<SectionType>(value.Trim(), true, out _))
            .WithMessage("SectionType is invalid.");

        RuleForEach(x => x.QuestionGroups)
            .SetValidator(new ImportQuestionGroupRequestValidator())
            .When(x => x.QuestionGroups != null);
    }
}
