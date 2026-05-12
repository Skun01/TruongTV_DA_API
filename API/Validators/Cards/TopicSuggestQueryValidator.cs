using Application.DTOs.Cards;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Cards;

public class TopicSuggestQueryValidator : AbstractValidator<TopicSuggestQuery>
{
    public TopicSuggestQueryValidator()
    {
        RuleFor(x => x.Topic)
            .NotEmpty()
            .WithMessage("Topic is required.")
            .MaximumLength(200)
            .WithMessage("Topic must not exceed 200 characters.");

        RuleFor(x => x.CardType)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<CardType>(value.Trim(), true, out _))
            .WithMessage("CardType is invalid.");

        RuleFor(x => x.Level)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<JlptLevel>(value.Trim(), true, out _))
            .WithMessage("Level is invalid.");
    }
}
