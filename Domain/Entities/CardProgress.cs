using Domain.Enums;

namespace Domain.Entities;

public class CardProgress : BaseEntity
{
    public string UserId { set; get; } = null!;
    public string CardId { set; get; } = null!;
    public DeckType CardType { set; get; }
    public CardStatus Status { set; get; } = CardStatus.New;
    public int SrsLevel { set; get; } = 0;
    public int CorrectStreak { set; get; } = 0;
    public int TotalReviews { set; get; } = 0;
    public int CorrectReviews { set; get; } = 0;
    public int NextExampleIndex { set; get; } = 0;
    public DateTime? LearnedAt { set; get; }
    public DateTime? LastReviewedAt { set; get; }
    public DateTime? NextReviewAt { set; get; }

    public virtual User User { set; get; } = null!;
}
