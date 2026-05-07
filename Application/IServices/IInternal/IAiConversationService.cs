using Application.DTOs.Ai;

namespace Application.IServices.IInternal;

public interface IAiConversationService
{
    Task<AiGeneratedJsonResult> GenerateConversationJsonAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
