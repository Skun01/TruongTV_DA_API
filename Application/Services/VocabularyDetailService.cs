using Application.Common;
using Application.DTOs.CardNotes;
using Application.DTOs.Vocabulary;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.IServices.IInternal;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class VocabularyDetailService : IVocabularyDetailService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVoicevoxService _voicevoxService;

    public VocabularyDetailService(IUnitOfWork unitOfWork, IVoicevoxService voicevoxService)
    {
        _unitOfWork = unitOfWork;
        _voicevoxService = voicevoxService;
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
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);

        var levelEnum = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);
        var statusEnum = EnumParsingHelper.ParseNullable<PublishStatus>(query.Status);
        var wordTypeEnum = EnumParsingHelper.ParseNullable<WordType>(query.WordType);
        var createdBy = query.CreatedByMe ? currentUserId : null;

        var (items, total) = await _unitOfWork.Cards.SearchVocabularyAsync(
            query.Q,
            levelEnum,
            statusEnum,
            wordTypeEnum,
            query.HasAudio,
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
        var writing = request.Writing.Trim();
        var inputAudioUrl = StringHelper.NormalizeOptional(request.AudioUrl);
        var shouldGenerateAudio = string.IsNullOrWhiteSpace(inputAudioUrl);
        var synthesisResult = shouldGenerateAudio
            ? await _voicevoxService.SynthesizeAsync(writing, request.SpeakerId)
            : null;

        var audioUrl = inputAudioUrl ?? synthesisResult?.AudioUrl;
        var finalPitchPattern = (request.PitchPattern == null || request.PitchPattern.Count == 0)
            ? synthesisResult?.PitchPattern
            : request.PitchPattern;
        var speakerId = request.SpeakerId ?? synthesisResult?.SpeakerId;

        var card = new Card
        {
            Id = cardId,
            CardType = CardType.Vocab,
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level),
            Tags = StringHelper.NormalizeList(request.Tags),
            Status = EnumParsingHelper.ParseNullable<PublishStatus>(request.Status) ?? PublishStatus.Draft,
            CreatedBy = currentUserId,
        };

        card.VocabularyDetail = new VocabularyDetail
        {
            CardId = cardId,
            Writing = writing,
            Reading = StringHelper.NormalizeOptional(request.Reading),
            PitchAccent = VocabularyHelper.SerializePitchPattern(finalPitchPattern),
            AudioUrl = audioUrl,
            SpeakerId = speakerId,
            WordType = EnumParsingHelper.ParseNullable<WordType>(request.WordType),
            Meanings = VocabularyHelper.MapMeaningItems(request.Meanings),
            Synonyms = StringHelper.NormalizeList(request.Synonyms),
            Antonyms = StringHelper.NormalizeList(request.Antonyms),
            RelatedPhrases = StringHelper.NormalizeList(request.RelatedPhrases),
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

        var writing = request.Writing.Trim();
        var inputAudioUrl = StringHelper.NormalizeOptional(request.AudioUrl);
        var shouldGenerateAudio = string.IsNullOrWhiteSpace(inputAudioUrl);
        var synthesisResult = shouldGenerateAudio
            ? await _voicevoxService.SynthesizeAsync(writing, request.SpeakerId)
            : null;

        var audioUrl = inputAudioUrl ?? synthesisResult?.AudioUrl;
        var finalPitchPattern = (request.PitchPattern == null || request.PitchPattern.Count == 0)
            ? synthesisResult?.PitchPattern
            : request.PitchPattern;
        var speakerId = request.SpeakerId ?? synthesisResult?.SpeakerId ?? detail.SpeakerId;

        card.Title = request.Title.Trim();
        card.Summary = request.Summary.Trim();
        card.Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level);
        card.Tags = StringHelper.NormalizeList(request.Tags);
        card.Status = EnumParsingHelper.ParseNullable<PublishStatus>(request.Status) ?? card.Status;
        card.UpdatedAt = DateTime.UtcNow;

        detail.Writing = writing;
        detail.Reading = StringHelper.NormalizeOptional(request.Reading);
        detail.PitchAccent = VocabularyHelper.SerializePitchPattern(finalPitchPattern);
        detail.AudioUrl = audioUrl;
        detail.SpeakerId = speakerId;
        detail.WordType = EnumParsingHelper.ParseNullable<WordType>(request.WordType);
        detail.Meanings = VocabularyHelper.MapMeaningItems(request.Meanings);
        detail.Synonyms = StringHelper.NormalizeList(request.Synonyms);
        detail.Antonyms = StringHelper.NormalizeList(request.Antonyms);
        detail.RelatedPhrases = StringHelper.NormalizeList(request.RelatedPhrases);

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
}
