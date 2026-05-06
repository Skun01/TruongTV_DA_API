using Domain.Enums;

namespace Domain.Entities;

public class ConversationMessage : BaseEntity
{
    public string ConversationId { get; set; } = string.Empty;
    public ConversationSession Conversation { get; set; } = null!;

    public MessageSender Sender { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string>? Suggestions { get; set; }
    public List<ExtractedVocabulary> NewVocabulary { get; set; } = new();
    public List<string> GrammarPoints { get; set; } = new();
}
