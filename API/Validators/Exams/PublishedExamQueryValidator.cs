using Application.DTOs.Exams;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Exams;

public class PublishedExamQueryValidator : AbstractValidator<PublishedExamQuery>
{
    public PublishedExamQueryValidator()
    {
        RuleFor(x => x.Keyword)
            .MaximumLength(200)
            .When(x => x.Keyword != null);

        RuleFor(x => x.Level)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<JlptLevel>(value.Trim(), true, out _))
            .WithMessage("Level is invalid.");
    }
}
