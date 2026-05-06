namespace Application.DTOs.Conversations;

public class ScenarioListResponse
{
    public List<ScenarioItemDto> Scenarios { get; set; } = new();
}

public class ScenarioItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
