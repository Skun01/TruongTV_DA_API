namespace Application.DTOs.Conversations;

public class AiMessageDto
{
    public string Text { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
}
