using Domain.Enums;

namespace Application.DTOs.Conversations;

public class ConversationMessageItemResponse
{
    public string MessageId { get; set; } = string.Empty;
    public MessageSender Sender { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
    public List<VocabularyItemDto> NewVocabulary { get; set; } = new();
    public List<string> GrammarPoints { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
