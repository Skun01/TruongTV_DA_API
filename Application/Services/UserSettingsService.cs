using Application.DTOs.UserSettings;
using Application.IRepositories;
using Application.IServices;
using Domain.Entities;

namespace Application.Services;

public class UserSettingsService : IUserSettingsService
{
    private readonly IUnitOfWork _unitOfWork;
    public UserSettingsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserSettingsDTO> GetSettingsAsync(string userId)
    {
        var settings = await GetOrCreateSettingsAsync(userId);

        return new UserSettingsDTO
        {
            DailyGoal = settings.DailyGoal,
            BatchSize = settings.BatchSize,
            CurrentStreak = settings.CurrentStreak,
            LongestStreak = settings.LongestStreak,
            LastStudyDate = settings.LastStudyDate
        };
    }

    public async Task<bool> UpdateSettingsAsync(UpdateUserSettingsRequest request, string userId)
    {
        var settings = await GetOrCreateSettingsAsync(userId);

        if (request.DailyGoal.HasValue)
            settings.DailyGoal = Math.Max(1, request.DailyGoal.Value);

        if (request.BatchSize.HasValue)
            settings.BatchSize = Math.Max(1, request.BatchSize.Value);

        settings.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // Lazy creation: tạo UserSettings nếu chưa có
    private async Task<UserSettings> GetOrCreateSettingsAsync(string userId)
    {
        var settings = await _unitOfWork.UserSettings.GetByUserIdAsync(userId);
        if (settings == null)
        {
            settings = new UserSettings
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId
            };
            await _unitOfWork.UserSettings.AddAsync(settings);
            await _unitOfWork.SaveChangesAsync();
        }
        return settings;
    }
}
