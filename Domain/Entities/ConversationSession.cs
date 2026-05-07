using Domain.Enums;

namespace Domain.Entities;

public class ConversationSession : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public string Scenario { get; set; } = string.Empty;
    public string? CustomScenario { get; set; }
    public JlptLevel Level { get; set; }
    public ConversationSessionStatus Status { get; set; } = ConversationSessionStatus.Active;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Feedback { get; set; }
    public DateTime? FeedbackGeneratedAt { get; set; }
    public string? ResultModel { get; set; }
    public string? ResultPromptVersion { get; set; }
    public int TotalMessages { get; set; }
    public int UserMessagesCount { get; set; }
    public int Score { get; set; }

    public ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
}
