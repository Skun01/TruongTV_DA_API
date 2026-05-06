using Domain.Enums;

namespace Application.DTOs.Conversations;

public class StartConversationRequest
{
    public string Scenario { get; set; } = string.Empty;
    public JlptLevel Level { get; set; }
    public string? CustomScenario { get; set; }
}
