using Application.DTOs.Questions;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Questions;

public class CreateQuestionRequestValidator : AbstractValidator<CreateQuestionRequest>
{
    public CreateQuestionRequestValidator()
    {
        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("GroupId is required.");

        RuleFor(x => x.QuestionText)
            .NotEmpty().WithMessage("QuestionText is required.")
            .MaximumLength(5000).WithMessage("QuestionText must not exceed 5000 characters.");

        RuleFor(x => x.Score)
            .GreaterThan(0).WithMessage("Score must be greater than 0.");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("OrderIndex must be >= 0.");

        RuleFor(x => x.Options)
            .NotEmpty().WithMessage("Options are required.")
            .Must(options => options.Count >= 2 && options.Count <= 4)
            .WithMessage("Options must have between 2 and 4 items.")
            .Must(options => options.Count(o => o.IsCorrect) == 1)
            .WithMessage("Exactly one option must be marked as correct.");

        RuleForEach(x => x.Options).ChildRules(option =>
        {
            option.RuleFor(o => o.Label)
                .NotEmpty().WithMessage("Label is required.")
                .Must(value => Enum.TryParse<OptionLabel>(value.Trim(), true, out _))
                .WithMessage("Label is invalid. Must be A, B, C, or D.");

            option.RuleFor(o => o.OptionType)
                .NotEmpty().WithMessage("OptionType is required.")
                .Must(value => Enum.TryParse<OptionType>(value.Trim(), true, out _))
                .WithMessage("OptionType is invalid.");
        });
    }
}
