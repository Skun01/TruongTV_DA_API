namespace Application.DTOs.Conversations;

public class SendMessageResponse
{
    public SendAiMessageDto AiMessage { get; set; } = null!;
    public ConversationSummaryDto Summary { get; set; } = null!;
}

public class SendAiMessageDto
{
    public string Text { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
    public List<VocabularyItemDto> NewVocabulary { get; set; } = new();
    public List<string> GrammarPoints { get; set; } = new();
}

public class VocabularyItemDto
{
    public string Word { get; set; } = string.Empty;
    public string Reading { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string Example { get; set; } = string.Empty;
}

public class ConversationSummaryDto
{
    public int TotalMessages { get; set; }
    public int UserMessagesCount { get; set; }
    public int NewWordsLearned { get; set; }
}
