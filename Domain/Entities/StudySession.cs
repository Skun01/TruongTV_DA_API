using Domain.Enums;

namespace Domain.Entities;

public class StudySession : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public string DeckId { get; set; } = string.Empty;
    public Deck Deck { get; set; } = null!;

    public StudyMode Mode { get; set; }
    public List<string> SelectedFolderIds { get; set; } = new();
    public List<string> CardIds { get; set; } = new();
    public List<string> CompletedCardIds { get; set; } = new();
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public DateTime? CompletedAt { get; set; }
}
