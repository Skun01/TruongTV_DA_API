namespace Application.DTOs.Dashboard.Learner;

public class DeckProgressResponse
{
    public List<DeckProgressItem> Decks { get; set; } = new();
}

public class DeckProgressItem
{
    public string DeckId { get; set; } = string.Empty;
    public string DeckTitle { get; set; } = string.Empty;
    public int TotalCards { get; set; }
    public int MasteredCards { get; set; }
    public int DueCards { get; set; }
    public int LearningCards { get; set; }
    public double CompletionPercent { get; set; }
}