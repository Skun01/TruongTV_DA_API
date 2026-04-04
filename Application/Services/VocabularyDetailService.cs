using Application.Common;
using Application.DTOs.CardNotes;
using Application.DTOs.Vocabulary;
using Application.IRepositories;
using Application.IServices;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Services;

public class VocabularyDetailService : IVocabularyDetailService
{
    private readonly IUnitOfWork _unitOfWork;

    public VocabularyDetailService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<VocabularyDetailResponse> GetDetailAsync(string cardId, string? currentUserId)
    {
        var card = await _unitOfWork.Cards.GetVocabularyDetailByIdAsync(cardId);
        if (card == null || card.VocabularyDetail == null || card.CardType != CardType.Vocab)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        EnsureCardReadable(card, currentUserId);

        var notes = await _unitOfWork.UserCardNotes.GetByCardIdWithRelationsAsync(cardId);

        return MapToDetailResponse(card, notes, currentUserId);
    }

    public async Task<(List<VocabularyListItemResponse> Items, MetaData Meta)> SearchAsync(
        string? q,
        string? level,
        string? status,
        bool createdByMe,
        int page,
        int pageSize,
        string currentUserId)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        var levelEnum = ParseLevel(level);
        var statusEnum = ParseStatus(status);
        var createdBy = createdByMe ? currentUserId : null;

        var (items, total) = await _unitOfWork.Cards.SearchVocabularyAsync(
            q,
            levelEnum,
            statusEnum,
            createdBy,
            page,
            pageSize);

        var mapped = items.Select(MapToListItemResponse).ToList();

        var meta = new MetaData
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
        };

        return (mapped, meta);
    }

    public async Task<VocabularyDetailResponse> CreateAsync(CreateVocabularyCardRequest request, string currentUserId)
    {
        var cardId = Guid.NewGuid().ToString();

        var card = new Card
        {
            Id = cardId,
            CardType = CardType.Vocab,
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            Level = ParseLevel(request.Level),
            Tags = NormalizeStringList(request.Tags),
            Status = ParseStatus(request.Status) ?? PublishStatus.Draft,
            CreatedBy = currentUserId,
        };

        card.VocabularyDetail = new VocabularyDetail
        {
            CardId = cardId,
            Writing = request.Writing.Trim(),
            Reading = NormalizeOptionalText(request.Reading),
            PitchAccent = SerializePitchPattern(request.PitchPattern),
            AudioUrl = NormalizeOptionalText(request.AudioUrl),
            WordType = ParseWordType(request.WordType),
            Meanings = MapMeaningItems(request.Meanings),
            Synonyms = NormalizeStringList(request.Synonyms),
            Antonyms = NormalizeStringList(request.Antonyms),
            RelatedPhrases = NormalizeStringList(request.RelatedPhrases),
        };

        await _unitOfWork.Cards.AddAsync(card);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Cards.GetVocabularyDetailByIdAsync(cardId)
            ?? throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        return MapToDetailResponse(created, new List<UserCardNote>(), currentUserId);
    }

    public async Task<VocabularyDetailResponse> UpdateAsync(string cardId, UpdateVocabularyCardRequest request, string currentUserId)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId);
        if (card == null || card.CardType != CardType.Vocab)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        var detail = await _unitOfWork.VocabularyDetails.GetByIdAsync(cardId);
        if (detail == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        card.Title = request.Title.Trim();
        card.Summary = request.Summary.Trim();
        card.Level = ParseLevel(request.Level);
        card.Tags = NormalizeStringList(request.Tags);
        card.Status = ParseStatus(request.Status) ?? card.Status;
        card.UpdatedAt = DateTime.UtcNow;

        detail.Writing = request.Writing.Trim();
        detail.Reading = NormalizeOptionalText(request.Reading);
        detail.PitchAccent = SerializePitchPattern(request.PitchPattern);
        detail.AudioUrl = NormalizeOptionalText(request.AudioUrl);
        detail.WordType = ParseWordType(request.WordType);
        detail.Meanings = MapMeaningItems(request.Meanings);
        detail.Synonyms = NormalizeStringList(request.Synonyms);
        detail.Antonyms = NormalizeStringList(request.Antonyms);
        detail.RelatedPhrases = NormalizeStringList(request.RelatedPhrases);

        _unitOfWork.Cards.UpdateAsync(card);
        _unitOfWork.VocabularyDetails.UpdateAsync(detail);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Cards.GetVocabularyDetailByIdAsync(cardId)
            ?? throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        var notes = await _unitOfWork.UserCardNotes.GetByCardIdWithRelationsAsync(cardId);
        return MapToDetailResponse(updated, notes, currentUserId);
    }

    public async Task<bool> SoftDeleteAsync(string cardId, string currentUserId)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId);
        if (card == null || card.CardType != CardType.Vocab)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        card.Status = PublishStatus.Archived;
        card.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Cards.UpdateAsync(card);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static VocabularyDetailResponse MapToDetailResponse(Card card, List<UserCardNote> notes, string? currentUserId)
    {
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
            Writing = card.VocabularyDetail?.Writing ?? string.Empty,
            Reading = card.VocabularyDetail?.Reading,
            PitchPattern = ParsePitchPattern(card.VocabularyDetail?.PitchAccent),
            AudioUrl = card.VocabularyDetail?.AudioUrl,
            WordType = card.VocabularyDetail?.WordType?.ToString(),
            Meanings = card.VocabularyDetail?.Meanings
                .Select(m => new VocabularyMeaningResponse
                {
                    PartOfSpeech = m.PartOfSpeech.ToString(),
                    Definitions = m.Definitions,
                })
                .ToList() ?? new List<VocabularyMeaningResponse>(),
            Synonyms = card.VocabularyDetail?.Synonyms ?? new List<string>(),
            Antonyms = card.VocabularyDetail?.Antonyms ?? new List<string>(),
            RelatedPhrases = card.VocabularyDetail?.RelatedPhrases ?? new List<string>(),
            Sentences = card.CardSentences
                .Select(cs => cs.Sentence)
                .Where(s => s != null)
                .Select(s => new VocabularySentenceResponse
                {
                    Id = s!.Id,
                    Text = s.Text,
                    Meaning = s.Meaning,
                    AudioUrl = s.AudioUrl,
                    Level = s.Level?.ToString(),
                })
                .ToList(),
            UserNotes = notes.Select(n => MapToNoteResponse(n, currentUserId)).ToList(),
        };
    }

    private static VocabularyListItemResponse MapToListItemResponse(Card card)
    {
        return new VocabularyListItemResponse
        {
            Id = card.Id,
            Title = card.Title,
            Summary = card.Summary,
            Level = card.Level?.ToString(),
            Tags = card.Tags,
            Status = card.Status.ToString(),
            CreatedAt = card.CreatedAt,
            UpdatedAt = card.UpdatedAt,
            Writing = card.VocabularyDetail?.Writing ?? string.Empty,
            Reading = card.VocabularyDetail?.Reading,
            WordType = card.VocabularyDetail?.WordType?.ToString(),
        };
    }

    private static void EnsureCardReadable(Card card, string? currentUserId)
    {
        if (card.Status != PublishStatus.Published && (string.IsNullOrWhiteSpace(currentUserId) || card.CreatedBy != currentUserId))
            throw new ApplicationException(MessageConstants.CommonMessage.UNAUTHORIZED);
    }

    private static CardNoteResponse MapToNoteResponse(UserCardNote note, string? currentUserId)
    {
        return new CardNoteResponse
        {
            Id = note.Id,
            UserId = note.UserId,
            UserName = note.User?.Username ?? string.Empty,
            Content = note.Content,
            LikesCount = note.LikesCount,
            IsLikedByMe = !string.IsNullOrWhiteSpace(currentUserId)
                && note.NoteLikes.Any(l => l.UserId == currentUserId),
            CreatedAt = note.CreatedAt,
        };
    }

    private static List<MeaningItem> MapMeaningItems(List<VocabularyMeaningRequest> meanings)
    {
        return meanings
            .Select(m => new MeaningItem
            {
                PartOfSpeech = ParsePartOfSpeech(m.PartOfSpeech),
                Definitions = NormalizeStringList(m.Definitions),
            })
            .ToList();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }

    private static List<string> NormalizeStringList(List<string>? values)
    {
        if (values == null || values.Count == 0)
            return new List<string>();

        return values
            .Select(v => v?.Trim())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string? SerializePitchPattern(List<int>? pitchPattern)
    {
        if (pitchPattern == null || pitchPattern.Count == 0)
            return null;

        return string.Join(",", pitchPattern);
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

    private static JlptLevel? ParseLevel(string? level)
    {
        if (string.IsNullOrWhiteSpace(level))
            return null;

        if (Enum.TryParse<JlptLevel>(level.Trim(), true, out var parsed))
            return parsed;

        throw new ApplicationException(MessageConstants.CommonMessage.INVALID);
    }

    private static PublishStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        if (Enum.TryParse<PublishStatus>(status.Trim(), true, out var parsed))
            return parsed;

        throw new ApplicationException(MessageConstants.CommonMessage.INVALID);
    }

    private static WordType? ParseWordType(string? wordType)
    {
        if (string.IsNullOrWhiteSpace(wordType))
            return null;

        if (Enum.TryParse<WordType>(wordType.Trim(), true, out var parsed))
            return parsed;

        throw new ApplicationException(MessageConstants.CommonMessage.INVALID);
    }

    private static PartOfSpeech ParsePartOfSpeech(string partOfSpeech)
    {
        if (Enum.TryParse<PartOfSpeech>(partOfSpeech.Trim(), true, out var parsed))
            return parsed;

        throw new ApplicationException(MessageConstants.CommonMessage.INVALID);
    }
}
