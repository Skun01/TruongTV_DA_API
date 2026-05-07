namespace Application.DTOs.Conversations;

public class SendMessageResponse
{
    public SendAiMessageDto AiMessage { get; set; } = null!;
    public ConversationSummaryDto Summary { get; set; } = null!;
}
