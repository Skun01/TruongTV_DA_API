using Application.DTOs.Exams;
using FluentValidation;

namespace API.Validators.Exams;

public class ImportExamQuestionRequestValidator : AbstractValidator<ImportExamQuestionRequest>
{
    public ImportExamQuestionRequestValidator()
    {
        RuleFor(x => x.QuestionText)
            .MaximumLength(5000)
            .When(x => !string.IsNullOrWhiteSpace(x.QuestionText));

        RuleFor(x => x.ImageUrl)
            .MaximumLength(512)
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

        RuleFor(x => x.ImageCaption)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.ImageCaption));

        RuleFor(x => x.Explanation)
            .MaximumLength(5000)
            .When(x => !string.IsNullOrWhiteSpace(x.Explanation));

        RuleForEach(x => x.Options)
            .SetValidator(new ImportQuestionOptionRequestValidator())
            .When(x => x.Options != null);
    }
}
