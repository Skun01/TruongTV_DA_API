using Application.Common;
using Application.DTOs.CardNotes;

namespace Application.IServices;

public interface ICardNoteService
{
    Task<(List<CardNoteResponse> Notes, MetaData Meta)> GetCardCommunityNotesAsync(string cardId, string currentUserId, int page, int pageSize);
    Task<CardNoteResponse> UpsertMyCardNoteAsync(string cardId, string userId, UpsertCardNoteRequest request);
    Task<bool> DeleteMyCardNoteAsync(string cardId, string userId);
    Task<ToggleCardNoteLikeResponse> ToggleCardNoteLikeAsync(string noteId, string userId);
}
