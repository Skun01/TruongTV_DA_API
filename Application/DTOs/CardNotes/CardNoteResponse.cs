namespace Application.DTOs.CardNotes;

public class CardNoteResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int LikesCount { get; set; }
    public bool IsLikedByMe { get; set; }
    public DateTime CreatedAt { get; set; }
}
