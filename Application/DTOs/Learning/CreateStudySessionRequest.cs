namespace Application.DTOs.Learning;

public class CreateStudySessionRequest
{
    public string DeckId { get; set; } = string.Empty;
    public List<string> FolderIds { get; set; } = new();
    public string Mode { get; set; } = string.Empty;
}
