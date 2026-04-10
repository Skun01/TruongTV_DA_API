using Application.DTOs.Vocabulary;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Vocabulary;

public class CreateVocabularyCardRequestValidator : AbstractValidator<CreateVocabularyCardRequest>
{
    public CreateVocabularyCardRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Summary)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.Writing)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Reading)
            .MaximumLength(200);

        RuleFor(x => x.SpeakerId)
            .GreaterThan(0)
            .When(x => x.SpeakerId.HasValue);

        RuleFor(x => x.WordType)
            .MaximumLength(50);

        RuleFor(x => x.WordType)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<WordType>(value.Trim(), true, out _))
            .WithMessage("WordType is invalid.");

        RuleFor(x => x.Level)
            .MaximumLength(10);

        RuleFor(x => x.Level)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<JlptLevel>(value.Trim(), true, out _))
            .WithMessage("Level is invalid.");

        RuleFor(x => x.Status)
            .MaximumLength(20);

        RuleFor(x => x.Status)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<PublishStatus>(value.Trim(), true, out _))
            .WithMessage("Status is invalid.");

        RuleFor(x => x.Tags)
            .Must(tags => tags.Count <= 20)
            .WithMessage("Tags cannot exceed 20 items.");

        RuleForEach(x => x.Tags)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Meanings)
            .NotEmpty();

        RuleForEach(x => x.Meanings)
            .SetValidator(new VocabularyMeaningRequestValidator());

        RuleForEach(x => x.Synonyms)
            .NotEmpty()
            .MaximumLength(200);

        RuleForEach(x => x.Antonyms)
            .NotEmpty()
            .MaximumLength(200);

        RuleForEach(x => x.RelatedPhrases)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Sentences)
            .Must(sentences => sentences.Count <= 20)
            .WithMessage("Sentences cannot exceed 20 items.");

        RuleForEach(x => x.Sentences)
            .SetValidator(new VocabularySentenceUpsertRequestValidator());
    }
}
