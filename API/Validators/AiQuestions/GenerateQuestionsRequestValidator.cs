using Application.DTOs.AiQuestions;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.AiQuestions;

public class GenerateQuestionsRequestValidator : AbstractValidator<GenerateQuestionsRequest>
{
    public GenerateQuestionsRequestValidator()
    {
        RuleFor(x => x.Level)
            .NotEmpty().WithMessage("Level is required.")
            .Must(value => Enum.TryParse<JlptLevel>(value.Trim(), true, out _))
            .WithMessage("Level is invalid.");

        RuleFor(x => x.SectionType)
            .NotEmpty().WithMessage("SectionType is required.")
            .Must(value => Enum.TryParse<SectionType>(value.Trim(), true, out _))
            .WithMessage("SectionType is invalid.");

        RuleFor(x => x.Topic)
            .NotEmpty().WithMessage("Topic is required.")
            .MaximumLength(500).WithMessage("Topic must not exceed 500 characters.");

        RuleFor(x => x.Count)
            .InclusiveBetween(1, 20).WithMessage("Count must be between 1 and 20.");

        RuleFor(x => x.QuestionGroupId)
            .MaximumLength(50).WithMessage("QuestionGroupId must not exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.QuestionGroupId));
    }
}
