using Application.DTOs.Learning;

namespace Application.IServices;

public interface IUserLearningSettingsService
{
    Task<StudySessionSettingsResponse> GetAsync(string userId);
    Task<StudySessionSettingsResponse> UpsertAsync(string userId, StudySessionSettingsRequest request);
}
