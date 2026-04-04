using Application.DTOs.CardNotes;
using Domain.Entities;

namespace Application.Mappings;

public static class CardNoteMappings
{
    public static CardNoteResponse ToCardNoteResponse(this UserCardNote note, string currentUserId)
    {
        return new CardNoteResponse
        {
            Id = note.Id,
            UserId = note.UserId,
            UserName = note.User?.Username ?? string.Empty,
            Content = note.Content,
            LikesCount = note.LikesCount,
            IsLikedByMe = note.NoteLikes.Any(l => l.UserId == currentUserId),
            CreatedAt = note.CreatedAt,
        };
    }

    public static ToggleCardNoteLikeResponse ToToggleLikeResponse(this UserCardNote note, bool isLiked)
    {
        return new ToggleCardNoteLikeResponse
        {
            IsLiked = isLiked,
            LikesCount = note.LikesCount,
        };
    }
}
