using Application.DTOs.Exams;
using FluentValidation;

namespace API.Validators.Exams;

public class CreateQuestionGroupRequestValidator : AbstractValidator<CreateQuestionGroupRequest>
{
    public CreateQuestionGroupRequestValidator()
    {
        RuleFor(x => x.Instruction)
            .NotEmpty().WithMessage("Instruction is required.")
            .MaximumLength(2000).WithMessage("Instruction must not exceed 2000 characters.");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("OrderIndex must be >= 0.");

        RuleFor(x => x.AudioUrl)
            .MaximumLength(512)
            .When(x => x.AudioUrl != null);

        RuleFor(x => x.PassageText)
            .MaximumLength(10000)
            .When(x => x.PassageText != null);
    }
}
