using Application.DTOs.Learning;
using Domain.Entities;

namespace Application.Mappings;

public static class UserLearningSettingsMappings
{
    public static StudySessionSettingsResponse ToResponse(this UserLearningSettings settings)
    {
        return new StudySessionSettingsResponse
        {
            FlashcardFront = settings.FlashcardFront.ToString(),
            FlashcardBack = settings.FlashcardBack.ToString(),
            MultipleChoiceQuestion = settings.MultipleChoiceQuestion.ToString(),
            ShuffleOptions = settings.ShuffleOptions,
        };
    }
}
