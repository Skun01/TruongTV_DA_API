namespace Application.IServices.IInternal;

public interface IAiConversationService
{
    Task<string> GenerateConversationJsonAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
