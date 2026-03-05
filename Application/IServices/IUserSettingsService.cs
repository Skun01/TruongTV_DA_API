using Application.DTOs.UserSettings;

namespace Application.IServices;

public interface IUserSettingsService
{
    Task<UserSettingsDTO> GetSettingsAsync(string userId);
    Task<bool> UpdateSettingsAsync(UpdateUserSettingsRequest request, string userId);
}
