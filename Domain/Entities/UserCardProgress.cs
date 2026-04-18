using Domain.Enums;

namespace Domain.Entities;

public class UserCardProgress
{
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public string CardId { get; set; } = string.Empty;
    public Card Card { get; set; } = null!;

    public SrsLevel SrsLevel { get; set; } = SrsLevel.Level1;
    public DateTime NextReviewAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReviewedAt { get; set; }
    public int ConsecutiveCorrect { get; set; }
    public bool IsMastered { get; set; }
    public string? LastSentenceId { get; set; }
    public Sentence? LastSentence { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
