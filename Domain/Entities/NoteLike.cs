namespace Domain.Entities;

public class NoteLike
{
    public string UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string NoteId { get; set; }
    public UserCardNote Note { get; set; } = null!;
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
