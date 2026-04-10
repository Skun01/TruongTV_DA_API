using Application.DTOs.Vocabulary;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Vocabulary;

public class VocabularySearchQueryValidator : AbstractValidator<VocabularySearchQuery>
{
    public VocabularySearchQueryValidator()
    {
        RuleFor(x => x.Level)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<JlptLevel>(value.Trim(), true, out _))
            .WithMessage("Level is invalid.");

        RuleFor(x => x.Status)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<PublishStatus>(value.Trim(), true, out _))
            .WithMessage("Status is invalid.");

        RuleFor(x => x.WordType)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<WordType>(value.Trim(), true, out _))
            .WithMessage("WordType is invalid.");
    }
}
