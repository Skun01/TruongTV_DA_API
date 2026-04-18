using Domain.Enums;

namespace Domain.Entities;

public class UserLearningSettings
{
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public FlashcardContentType FlashcardFront { get; set; } = FlashcardContentType.Title;
    public FlashcardContentType FlashcardBack { get; set; } = FlashcardContentType.Summary;
    public MultipleChoiceQuestionType MultipleChoiceQuestion { get; set; } = MultipleChoiceQuestionType.TitleToSummary;
    public bool ShuffleOptions { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
