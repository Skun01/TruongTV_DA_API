using Application.DTOs.ExampleSentence;

namespace Application.IServices;

public interface IExampleSentenceService
{
    Task<bool> CreateExampleSentence(CreateCardExampleRequest request, string userId);
    Task<bool> UpdateExampleAsync(UpdateCardExampleRequest request, string exampleId, string userId);
    Task<bool> DeleteExampleAsync(string id, string userId);
}
