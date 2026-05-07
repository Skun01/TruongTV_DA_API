namespace Application.DTOs.Conversations;

public class ConversationAiVocabularyContent
{
    public string Word { get; set; } = string.Empty;
    public string Reading { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string Example { get; set; } = string.Empty;
    public string JlptLevel { get; set; } = "N5";
}
