using Application.Common;
using Application.DTOs.CardNotes;
using Application.DTOs.Vocabulary;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
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

        return card.ToDetailResponse(notes, currentUserId);
    }

    public async Task<(List<VocabularyListItemResponse> Items, MetaData Meta)> SearchAsync(VocabularySearchQuery query, string currentUserId)
    {
        var page = query.Page;
        var pageSize = query.PageSize;
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        var levelEnum = ParseLevel(query.Level);
        var statusEnum = ParseStatus(query.Status);
        var createdBy = query.CreatedByMe ? currentUserId : null;

        var (items, total) = await _unitOfWork.Cards.SearchVocabularyAsync(
            query.Q,
            levelEnum,
            statusEnum,
            createdBy,
            page,
            pageSize);

        var mapped = items.Select(item => item.ToListItemResponse()).ToList();

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

        return created.ToDetailResponse(new List<UserCardNote>(), currentUserId);
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
        return updated.ToDetailResponse(notes, currentUserId);
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

    private static void EnsureCardReadable(Card card, string? currentUserId)
    {
        if (card.Status != PublishStatus.Published && (string.IsNullOrWhiteSpace(currentUserId) || card.CreatedBy != currentUserId))
            throw new ApplicationException(MessageConstants.CommonMessage.UNAUTHORIZED);
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
