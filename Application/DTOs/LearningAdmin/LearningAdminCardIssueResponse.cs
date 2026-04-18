namespace Application.DTOs.LearningAdmin;

public class LearningAdminCardIssueResponse
{
    public string CardId { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> AvailableModes { get; set; } = new();
    public List<LearningAdminCardIssueItemResponse> Issues { get; set; } = new();
}
