namespace Domain.Entities;

public class DeckQueue : BaseEntity
{
    public string UserId { set; get; } = null!;
    public string DeckId { set; get; } = null!;

    public virtual User User { set; get; } = null!;
    public virtual Deck Deck { set; get; } = null!;
}
