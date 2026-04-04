using Application.DTOs.Vocabulary;
using FluentValidation;

namespace API.Validators.Vocabulary;

public class VocabularyMeaningRequestValidator : AbstractValidator<VocabularyMeaningRequest>
{
    public VocabularyMeaningRequestValidator()
    {
        RuleFor(x => x.PartOfSpeech)
            .NotEmpty();

        RuleFor(x => x.Definitions)
            .NotEmpty();

        RuleForEach(x => x.Definitions)
            .NotEmpty()
            .MaximumLength(500);
    }
}
