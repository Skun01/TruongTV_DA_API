using Application.Common;
using Application.DTOs.CardNotes;
using Application.DTOs.Vocabulary;
using Application.IRepositories;
using Application.IServices;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class VocabularyDetailService : IVocabularyDetailService
{
    private readonly IUnitOfWork _unitOfWork;

    public VocabularyDetailService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<VocabularyDetailResponse> GetDetailAsync(string cardId, string currentUserId)
    {
        var card = await _unitOfWork.Cards.GetVocabularyDetailByIdAsync(cardId);
        if (card == null || card.VocabularyDetail == null || card.CardType != CardType.Vocab)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        EnsureCardReadable(card, currentUserId);

        var notes = await _unitOfWork.UserCardNotes.GetByCardIdWithRelationsAsync(cardId);

        return new VocabularyDetailResponse
        {
            Id = card.Id,
            CardType = card.CardType.ToString(),
            Title = card.Title,
            Summary = card.Summary,
            Level = card.Level?.ToString(),
            Tags = card.Tags,
            Status = card.Status.ToString(),
            CreatedAt = card.CreatedAt,
            UpdatedAt = card.UpdatedAt,
            Writing = card.VocabularyDetail.Writing,
            Reading = card.VocabularyDetail.Reading,
            PitchPattern = ParsePitchPattern(card.VocabularyDetail.PitchAccent),
            AudioUrl = card.VocabularyDetail.AudioUrl,
            WordType = card.VocabularyDetail.WordType?.ToString(),
            Meanings = card.VocabularyDetail.Meanings
                .Select(m => new VocabularyMeaningResponse
                {
                    PartOfSpeech = m.PartOfSpeech.ToString(),
                    Definitions = m.Definitions,
                })
                .ToList(),
            Synonyms = card.VocabularyDetail.Synonyms,
            Antonyms = card.VocabularyDetail.Antonyms,
            RelatedPhrases = card.VocabularyDetail.RelatedPhrases,
            Sentences = card.CardSentences
                .Select(cs => cs.Sentence)
                .Where(s => s != null)
                .Select(s => new VocabularySentenceResponse
                {
                    Id = s.Id,
                    Text = s.Text,
                    Meaning = s.Meaning,
                    AudioUrl = s.AudioUrl,
                    Level = s.Level?.ToString(),
                })
                .ToList(),
            UserNotes = notes.Select(n => MapToNoteResponse(n, currentUserId)).ToList(),
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

    private static List<int>? ParsePitchPattern(string? pitchAccent)
    {
        if (string.IsNullOrWhiteSpace(pitchAccent))
            return null;

        var normalized = pitchAccent
            .Replace("[", string.Empty)
            .Replace("]", string.Empty)
            .Trim();

        var parsed = normalized
            .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(value => int.TryParse(value, out var number) ? number : (int?)null)
            .Where(number => number.HasValue)
            .Select(number => number!.Value)
            .ToList();

        return parsed.Count == 0 ? null : parsed;
    }
}
