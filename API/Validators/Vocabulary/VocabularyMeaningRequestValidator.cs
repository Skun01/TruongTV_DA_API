using Application.DTOs.Vocabulary;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Vocabulary;

public class VocabularyMeaningRequestValidator : AbstractValidator<VocabularyMeaningRequest>
{
    public VocabularyMeaningRequestValidator()
    {
        RuleFor(x => x.PartOfSpeech)
            .NotEmpty();

        RuleFor(x => x.PartOfSpeech)
            .Must(value => !string.IsNullOrWhiteSpace(value) && Enum.TryParse<PartOfSpeech>(value.Trim(), true, out _))
            .WithMessage("PartOfSpeech is invalid.");

        RuleFor(x => x.Definitions)
            .NotEmpty();

        RuleForEach(x => x.Definitions)
            .NotEmpty()
            .MaximumLength(500);
    }
}
