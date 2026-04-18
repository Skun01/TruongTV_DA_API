using Application.DTOs.Learning;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class UserLearningSettingsService : IUserLearningSettingsService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserLearningSettingsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StudySessionSettingsResponse> GetAsync(string userId)
    {
        var settings = await _unitOfWork.UserLearningSettings.GetByUserIdAsync(userId)
            ?? CreateDefaultEntity(userId);

        return settings.ToResponse();
    }

    public async Task<StudySessionSettingsResponse> UpsertAsync(string userId, StudySessionSettingsRequest request)
    {
        var settings = await _unitOfWork.UserLearningSettings.GetByUserIdAsync(userId);
        var isNew = settings == null;
        settings ??= CreateDefaultEntity(userId);

        if (!string.IsNullOrWhiteSpace(request.FlashcardFront))
            settings.FlashcardFront = EnumParsingHelper.ParseRequired<FlashcardContentType>(request.FlashcardFront);

        if (!string.IsNullOrWhiteSpace(request.FlashcardBack))
            settings.FlashcardBack = EnumParsingHelper.ParseRequired<FlashcardContentType>(request.FlashcardBack);

        if (!string.IsNullOrWhiteSpace(request.MultipleChoiceQuestion))
            settings.MultipleChoiceQuestion = EnumParsingHelper.ParseRequired<MultipleChoiceQuestionType>(request.MultipleChoiceQuestion);

        if (request.ShuffleOptions.HasValue)
            settings.ShuffleOptions = request.ShuffleOptions.Value;

        settings.UpdatedAt = DateTime.UtcNow;

        if (isNew)
            await _unitOfWork.UserLearningSettings.AddAsync(settings);
        else
            _unitOfWork.UserLearningSettings.UpdateAsync(settings);

        await _unitOfWork.SaveChangesAsync();
        return settings.ToResponse();
    }

    private static UserLearningSettings CreateDefaultEntity(string userId)
    {
        return new UserLearningSettings
        {
            UserId = userId,
            FlashcardFront = FlashcardContentType.Title,
            FlashcardBack = FlashcardContentType.Summary,
            MultipleChoiceQuestion = MultipleChoiceQuestionType.TitleToSummary,
            ShuffleOptions = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
