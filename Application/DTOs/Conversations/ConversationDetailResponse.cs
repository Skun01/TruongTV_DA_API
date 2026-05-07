using Domain.Enums;

namespace Application.DTOs.Conversations;

public class ConversationDetailResponse
{
    public string ConversationId { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
    public string? CustomScenario { get; set; }
    public string ScenarioText { get; set; } = string.Empty;
    public JlptLevel Level { get; set; }
    public ConversationSessionStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalMessages { get; set; }
    public int UserMessagesCount { get; set; }
    public int Score { get; set; }
    public List<ConversationMessageItemResponse> Messages { get; set; } = new();
}
