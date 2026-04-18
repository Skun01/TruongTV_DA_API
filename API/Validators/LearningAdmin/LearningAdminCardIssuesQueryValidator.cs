using Application.DTOs.LearningAdmin;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.LearningAdmin;

public class LearningAdminCardIssuesQueryValidator : AbstractValidator<LearningAdminCardIssuesQuery>
{
    public LearningAdminCardIssuesQueryValidator()
    {
        RuleFor(x => x.CardType)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<CardType>(value.Trim(), true, out _))
            .WithMessage("CardType is invalid.");

        RuleFor(x => x.Mode)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<StudyMode>(value.Trim(), true, out _))
            .WithMessage("Mode is invalid.");

        RuleFor(x => x.IssueType)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<LearningIssueType>(value.Trim(), true, out _))
            .WithMessage("IssueType is invalid.");

        RuleFor(x => x.Q)
            .MaximumLength(200)
            .When(x => x.Q != null);

        RuleFor(x => x.DeckId)
            .MaximumLength(50)
            .When(x => x.DeckId != null);
    }
}
