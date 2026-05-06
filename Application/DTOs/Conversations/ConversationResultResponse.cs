using Domain.Enums;

namespace Application.DTOs.Conversations;

public class ConversationResultResponse
{
    public string ConversationId { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
    public JlptLevel Level { get; set; }
    public string Duration { get; set; } = string.Empty;
    public int TotalMessages { get; set; }
    public List<VocabularyItemDto> NewVocabulary { get; set; } = new();
    public List<string> GrammarPoints { get; set; } = new();
    public string Feedback { get; set; } = string.Empty;
    public int Score { get; set; }
}
