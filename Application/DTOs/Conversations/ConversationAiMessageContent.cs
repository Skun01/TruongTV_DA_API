namespace Application.DTOs.Conversations;

public class ConversationAiMessageContent
{
    public string Text { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
    public List<ConversationAiVocabularyContent> NewVocabulary { get; set; } = new();
    public List<string> GrammarPoints { get; set; } = new();
}
