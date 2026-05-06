namespace Application.DTOs.Conversations;

public class StartConversationResponse
{
    public string ConversationId { get; set; } = string.Empty;
    public AiMessageDto AiMessage { get; set; } = null!;
}
