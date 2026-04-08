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
        var reading = StringHelper.NormalizeOptional(request.Reading);
        var synthesisText = ResolveVocabularySynthesisText(writing, reading);
        var synthesisResult = await _voicevoxService.SynthesizeAsync(synthesisText, request.SpeakerId)
            ?? throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        var finalPitchPattern = (request.PitchPattern == null || request.PitchPattern.Count == 0)
            ? synthesisResult.PitchPattern
            : request.PitchPattern;

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
            Reading = reading,
            PitchAccent = VocabularyHelper.SerializePitchPattern(finalPitchPattern),
            AudioUrl = synthesisResult.AudioUrl,
            SpeakerId = synthesisResult.SpeakerId,
            WordType = EnumParsingHelper.ParseNullable<WordType>(request.WordType),
            Meanings = VocabularyHelper.MapMeaningItems(request.Meanings),
            Synonyms = StringHelper.NormalizeList(request.Synonyms),
            Antonyms = StringHelper.NormalizeList(request.Antonyms),
            RelatedPhrases = StringHelper.NormalizeList(request.RelatedPhrases),
        };

        await _unitOfWork.Cards.AddAsync(card);
        await SyncVocabularySentencesAsync(cardId, request.Sentences, currentUserId);
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
        var reading = StringHelper.NormalizeOptional(request.Reading);
        var synthesisText = ResolveVocabularySynthesisText(writing, reading);
        var synthesisResult = await _voicevoxService.SynthesizeAsync(synthesisText, request.SpeakerId)
            ?? throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        var finalPitchPattern = (request.PitchPattern == null || request.PitchPattern.Count == 0)
            ? synthesisResult.PitchPattern
            : request.PitchPattern;

        card.Title = request.Title.Trim();
        card.Summary = request.Summary.Trim();
        card.Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level);
        card.Tags = StringHelper.NormalizeList(request.Tags);
        card.Status = EnumParsingHelper.ParseNullable<PublishStatus>(request.Status) ?? card.Status;
        card.UpdatedAt = DateTime.UtcNow;

        detail.Writing = writing;
        detail.Reading = reading;
        detail.PitchAccent = VocabularyHelper.SerializePitchPattern(finalPitchPattern);
        detail.AudioUrl = synthesisResult.AudioUrl;
        detail.SpeakerId = synthesisResult.SpeakerId;
        detail.WordType = EnumParsingHelper.ParseNullable<WordType>(request.WordType);
        detail.Meanings = VocabularyHelper.MapMeaningItems(request.Meanings);
        detail.Synonyms = StringHelper.NormalizeList(request.Synonyms);
        detail.Antonyms = StringHelper.NormalizeList(request.Antonyms);
        detail.RelatedPhrases = StringHelper.NormalizeList(request.RelatedPhrases);

        await SyncVocabularySentencesAsync(cardId, request.Sentences, currentUserId);
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

    private static string ResolveVocabularySynthesisText(string writing, string? reading)
    {
        return string.IsNullOrWhiteSpace(reading) ? writing : reading;
    }

    private async Task SyncVocabularySentencesAsync(
        string cardId,
        List<VocabularySentenceUpsertRequest> requests,
        string currentUserId)
    {
        var existingLinks = await _unitOfWork.CardSentences.GetByCardIdAsync(cardId);
        var keptSentenceIds = new HashSet<string>();

        foreach (var request in requests)
        {
            var sentence = await UpsertVocabularySentenceAsync(request, currentUserId);
            if (!keptSentenceIds.Add(sentence.Id))
                continue;

            if (existingLinks.Any(link => link.SentenceId == sentence.Id))
                continue;

            await _unitOfWork.CardSentences.AddAsync(new CardSentence
            {
                CardId = cardId,
                SentenceId = sentence.Id,
            });
        }

        foreach (var link in existingLinks.Where(link => !keptSentenceIds.Contains(link.SentenceId)))
        {
            _unitOfWork.CardSentences.DeleteAsync(link);
        }
    }

    private async Task<Sentence> UpsertVocabularySentenceAsync(
        VocabularySentenceUpsertRequest request,
        string currentUserId)
    {
        var text = request.Text.Trim();
        var synthesisResult = await _voicevoxService.SynthesizeAsync(text, request.SpeakerId)
            ?? throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        if (string.IsNullOrWhiteSpace(request.Id))
        {
            var sentence = new Sentence
            {
                Id = Guid.NewGuid().ToString(),
                Text = text,
                Meaning = request.Meaning.Trim(),
                AudioUrl = synthesisResult.AudioUrl,
                SpeakerId = synthesisResult.SpeakerId,
                Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level),
                CreatedBy = currentUserId,
            };

            await _unitOfWork.Sentences.AddAsync(sentence);
            return sentence;
        }

        var existingSentence = await _unitOfWork.Sentences.GetByIdAsync(request.Id);
        if (existingSentence == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        existingSentence.Text = text;
        existingSentence.Meaning = request.Meaning.Trim();
        existingSentence.AudioUrl = synthesisResult.AudioUrl;
        existingSentence.SpeakerId = synthesisResult.SpeakerId;
        existingSentence.Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level);
        existingSentence.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Sentences.UpdateAsync(existingSentence);
        return existingSentence;
    }
}
