using Application.DTOs.Questions;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Questions;

public class QuestionSearchQueryValidator : AbstractValidator<QuestionSearchQuery>
{
    public QuestionSearchQueryValidator()
    {
        RuleFor(x => x.Keyword)
            .MaximumLength(200)
            .When(x => x.Keyword != null);

        RuleFor(x => x.Level)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<JlptLevel>(value.Trim(), true, out _))
            .WithMessage("Level is invalid.");

        RuleFor(x => x.SectionType)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<SectionType>(value.Trim(), true, out _))
            .WithMessage("SectionType is invalid.");
    }
}
