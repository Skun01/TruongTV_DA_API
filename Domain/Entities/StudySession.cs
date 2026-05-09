using Domain.Enums;

namespace Domain.Entities;

public class StudySession : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public string? DeckId { get; set; }
    public Deck? Deck { get; set; }

    public StudyMode Mode { get; set; }
    public FlashcardContentType FlashcardFront { get; set; } = FlashcardContentType.Title;
    public FlashcardContentType FlashcardBack { get; set; } = FlashcardContentType.Summary;
    public MultipleChoiceQuestionType MultipleChoiceQuestion { get; set; } = MultipleChoiceQuestionType.TitleToSummary;
    public bool ShuffleOptions { get; set; } = true;
    public List<string> SelectedFolderIds { get; set; } = new();
    public List<string> CardIds { get; set; } = new();
    public List<string> CompletedCardIds { get; set; } = new();
    public List<string> RetryCardIds { get; set; } = new();
    public List<string> SkippedCardIds { get; set; } = new();
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public DateTime? CompletedAt { get; set; }
}
