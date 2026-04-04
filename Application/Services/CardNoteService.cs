using Application.Common;
using Application.DTOs.CardNotes;
using Application.IRepositories;
using Application.IServices;
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

    public async Task<(List<CardNoteResponse> Notes, MetaData Meta)> GetCardCommunityNotesAsync(string cardId, string currentUserId, int page, int pageSize)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId);
        if (card == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        EnsureCardReadable(card, currentUserId);

        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var (notes, total) = await _unitOfWork.UserCardNotes.GetCommunityByCardIdAsync(cardId, currentUserId, page, pageSize);
        var mappedNotes = notes.Select(n => MapToNoteResponse(n, currentUserId)).ToList();

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

        return MapToNoteResponse(savedNote, userId);
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

        return new ToggleCardNoteLikeResponse
        {
            IsLiked = isLiked,
            LikesCount = note.LikesCount,
        };
    }

    private static void EnsureCardReadable(Card card, string currentUserId)
    {
        if (card.Status != PublishStatus.Published && card.CreatedBy != currentUserId)
            throw new ApplicationException(MessageConstants.CommonMessage.UNAUTHORIZED);
    }

    private static CardNoteResponse MapToNoteResponse(UserCardNote note, string currentUserId)
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
}
