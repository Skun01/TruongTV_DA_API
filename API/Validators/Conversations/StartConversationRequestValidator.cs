using Application.DTOs.Conversations;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Conversations;

public class StartConversationRequestValidator : AbstractValidator<StartConversationRequest>
{
    private static readonly string[] ValidScenarios = { "Shopping", "Interview", "Direction", "Meeting", "Restaurant", "Custom" };

    public StartConversationRequestValidator()
    {
        RuleFor(x => x.Scenario)
            .NotEmpty().WithMessage("Scenario is required.")
            .MaximumLength(50).WithMessage("Scenario must not exceed 50 characters.");

        RuleFor(x => x.Scenario)
            .Must(s => ValidScenarios.Contains(s, StringComparer.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrWhiteSpace(x.Scenario))
            .WithMessage("Scenario must be one of: Shopping, Interview, Direction, Meeting, Restaurant, Custom.");

        RuleFor(x => x.Level)
            .IsInEnum().WithMessage("Level must be a valid JLPT level (N5-N1).");

        RuleFor(x => x.CustomScenario)
            .MaximumLength(500).WithMessage("Custom scenario must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.CustomScenario));

        RuleFor(x => x.CustomScenario)
            .NotEmpty().WithMessage("Custom scenario is required when scenario is 'Custom'.")
            .When(x => x.Scenario.Equals("Custom", StringComparison.OrdinalIgnoreCase));
    }
}
