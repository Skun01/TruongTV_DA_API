namespace Application.DTOs.Conversations;

public class ConversationSummaryDto
{
    public int TotalMessages { get; set; }
    public int UserMessagesCount { get; set; }
    public int NewWordsLearned { get; set; }
}
