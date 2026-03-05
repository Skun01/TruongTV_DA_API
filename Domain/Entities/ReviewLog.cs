namespace Domain.Entities;

public class ReviewLog : BaseEntity
{
    public string UserId { set; get; } = null!;
    public string? ExampleSentenceId { set; get; }
    public string CardProgressId { set; get; } = null!;
    public bool IsCorrect { set; get; }
    public string? UserAnswer { set; get; }
    public bool IsGhost { set; get; } = false;

    public virtual User User { set; get; } = null!;
    public virtual ExampleSentence? ExampleSentence { set; get; }
    public virtual CardProgress CardProgress { set; get; } = null!;
}
