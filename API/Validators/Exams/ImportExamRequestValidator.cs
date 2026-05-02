using Application.DTOs.Exams;
using FluentValidation;

namespace API.Validators.Exams;

public class ImportExamRequestValidator : AbstractValidator<ImportExamRequest>
{
    public ImportExamRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.Level)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.Level));

        RuleForEach(x => x.Sections)
            .SetValidator(new ImportExamSectionRequestValidator())
            .When(x => x.Sections != null);
    }
}
