namespace Application.DTOs.LearningAdmin;

public class UserLearningDeckProgressResponse
{
    public string DeckId { get; set; } = string.Empty;
    public string DeckTitle { get; set; } = string.Empty;
    public int TrackedCards { get; set; }
    public int MasteredCards { get; set; }
    public int DueCards { get; set; }
}
