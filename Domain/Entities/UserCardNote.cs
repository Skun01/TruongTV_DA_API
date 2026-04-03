namespace Domain.Entities;

public class UserCardNote : BaseEntity
{
    public string UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string CardId { get; set; }
    public Card Card { get; set; } = null!;
    
    public string Content { get; set; } = string.Empty;
    public int LikesCount { get; set; } = 0;
    
    // Navigation
    public ICollection<NoteLike> NoteLikes { get; set; } = new List<NoteLike>();
}
