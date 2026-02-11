using Application.DTOs.ExampleSentence;

namespace Application.IServices;

public interface IExampleSentenceService
{
    Task<bool> CreateExampleSentence(CreateCardExampleRequest request, string userId);
}
