namespace Application.DTOs.Deck;

public class DeckProgressDTO
{
    public int TotalCards { set; get; }
    public int NewCards { set; get; }
    public int LearningCards { set; get; }
    public int MasteredCards { set; get; }
    public int DueForReview { set; get; }
    public double AccuracyPercent { set; get; }
}
