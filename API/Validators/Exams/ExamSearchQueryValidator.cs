using Application.DTOs.Exams;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Exams;

public class ExamSearchQueryValidator : AbstractValidator<ExamSearchQuery>
{
    public ExamSearchQueryValidator()
    {
        RuleFor(x => x.Keyword)
            .MaximumLength(200)
            .When(x => x.Keyword != null);

        RuleFor(x => x.Level)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<JlptLevel>(value.Trim(), true, out _))
            .WithMessage("Level is invalid.");

        RuleFor(x => x.Status)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<PublishStatus>(value.Trim(), true, out _))
            .WithMessage("Status is invalid.");
    }
}
