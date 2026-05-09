using Application.Common;
using Application.DTOs.CardNotes;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class CardNoteService : ICardNoteService
{
    private readonly IUnitOfWork _unitOfWork;

    public CardNoteService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(List<CardNoteResponse> Notes, MetaData Meta)> GetCardCommunityNotesAsync(string cardId, string currentUserId, CardNoteListQuery query)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId);
        if (card == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        EnsureCardReadable(card, currentUserId);

        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize, 10, 100);

        var (notes, total) = await _unitOfWork.UserCardNotes.GetCommunityByCardIdAsync(cardId, currentUserId, page, pageSize);
        var mappedNotes = notes.Select(n => n.ToCardNoteResponse(currentUserId)).ToList();

        var meta = new MetaData
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
        };

        return (mappedNotes, meta);
    }

    public async Task<CardNoteResponse> UpsertMyCardNoteAsync(string cardId, string userId, UpsertCardNoteRequest request)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId);
        if (card == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        EnsureCardReadable(card, userId);

        var myNote = await _unitOfWork.UserCardNotes.GetMyNoteByCardIdAsync(cardId, userId);
        if (myNote == null)
        {
            myNote = new UserCardNote
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                CardId = cardId,
                Content = request.Content.Trim(),
                LikesCount = 0,
            };

            await _unitOfWork.UserCardNotes.AddAsync(myNote);
        }
        else
        {
            myNote.Content = request.Content.Trim();
            myNote.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.UserCardNotes.UpdateAsync(myNote);
        }

        await _unitOfWork.SaveChangesAsync();

        var savedNote = await _unitOfWork.UserCardNotes.GetMyNoteByCardIdAsync(cardId, userId);
        if (savedNote == null)
            throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        return savedNote.ToCardNoteResponse(userId);
    }

    public async Task<bool> DeleteMyCardNoteAsync(string cardId, string userId)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId);
        if (card == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        var myNote = await _unitOfWork.UserCardNotes.GetMyNoteByCardIdAsync(cardId, userId);
        if (myNote == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        _unitOfWork.UserCardNotes.DeleteAsync(myNote);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<ToggleCardNoteLikeResponse> ToggleCardNoteLikeAsync(string noteId, string userId)
    {
        var note = await _unitOfWork.UserCardNotes.GetByIdWithRelationsAsync(noteId);
        if (note == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        var existingLike = await _unitOfWork.NoteLikes.GetByUserAndNoteIdAsync(userId, noteId);
        var isLiked = existingLike == null;

        if (existingLike == null)
        {
            await _unitOfWork.NoteLikes.AddAsync(new NoteLike
            {
                UserId = userId,
                NoteId = noteId,
            });
            note.LikesCount += 1;
        }
        else
        {
            _unitOfWork.NoteLikes.DeleteAsync(existingLike);
            note.LikesCount = Math.Max(0, note.LikesCount - 1);
        }

        note.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.UserCardNotes.UpdateAsync(note);
        await _unitOfWork.SaveChangesAsync();

        return note.ToToggleLikeResponse(isLiked);
    }

    private static void EnsureCardReadable(Card card, string currentUserId)
    {
        if (card.Status != PublishStatus.Published && card.CreatedBy != currentUserId)
            throw new ApplicationException(MessageConstants.CommonMessage.UNAUTHORIZED);
    }

}
