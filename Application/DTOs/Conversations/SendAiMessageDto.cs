namespace Application.DTOs.Conversations;

public class SendAiMessageDto
{
    public string Text { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
    public List<VocabularyItemDto> NewVocabulary { get; set; } = new();
    public List<string> GrammarPoints { get; set; } = new();
}
