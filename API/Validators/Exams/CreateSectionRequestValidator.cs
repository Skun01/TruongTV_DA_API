using Application.DTOs.Exams;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Exams;

public class CreateSectionRequestValidator : AbstractValidator<CreateSectionRequest>
{
    public CreateSectionRequestValidator()
    {
        RuleFor(x => x.SectionType)
            .NotEmpty().WithMessage("SectionType is required.")
            .Must(value => Enum.TryParse<SectionType>(value.Trim(), true, out _))
            .WithMessage("SectionType is invalid.");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("OrderIndex must be >= 0.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("DurationMinutes must be greater than 0.");

        RuleFor(x => x.MaxScore)
            .GreaterThan(0).WithMessage("MaxScore must be greater than 0.");

        RuleFor(x => x.PassScore)
            .GreaterThanOrEqualTo(0).WithMessage("PassScore must be >= 0.")
            .LessThanOrEqualTo(x => x.MaxScore).WithMessage("PassScore must not exceed MaxScore.");
    }
}
