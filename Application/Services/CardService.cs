using Application.Common;
using Application.DTOs.Cards;
using Application.DTOs.Grammar;
using Application.DTOs.Kanji;
using Application.DTOs.Vocabulary;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.IServices.IInternal;
using Application.Mappings;
using Domain.Constants;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class CardService : ICardService
{
    private static readonly TimeSpan AiTimeout = TimeSpan.FromSeconds(30);

    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiGenerationService _aiGenerationService;
    private readonly ILogger<CardService> _logger;
    private readonly IVocabularyDetailService _vocabularyDetailService;
    private readonly IGrammarService _grammarService;
    private readonly IKanjiService _kanjiService;

    public CardService(
        IUnitOfWork unitOfWork,
        IAiGenerationService aiGenerationService,
        ILogger<CardService> logger,
        IVocabularyDetailService vocabularyDetailService,
        IGrammarService grammarService,
        IKanjiService kanjiService)
    {
        _unitOfWork = unitOfWork;
        _aiGenerationService = aiGenerationService;
        _logger = logger;
        _vocabularyDetailService = vocabularyDetailService;
        _grammarService = grammarService;
        _kanjiService = kanjiService;
    }

    public async Task<(List<CardListItemResponse> Items, MetaData Meta)> SearchAsync(CardSearchQuery query)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);
        var cardTypeEnum = EnumParsingHelper.ParseNullable<CardType>(query.CardType);

        if (cardTypeEnum == CardType.Vocab)
            return await SearchVocabularyOnlyAsync(query.Q, query.Level, page, pageSize);

        if (cardTypeEnum == CardType.Grammar)
            return await SearchGrammarOnlyAsync(query.Q, query.Level, page, pageSize);

        if (cardTypeEnum == CardType.Kanji)
            return await SearchKanjiOnlyAsync(query.Q, query.Level, page, pageSize);

        var vocabularyItems = await SearchAllVocabularyPublishedAsync(query.Q, query.Level);
        var grammarItems = await SearchAllGrammarPublishedAsync(query.Q, query.Level);
        var kanjiItems = await SearchAllKanjiPublishedAsync(query.Q, query.Level);

        var merged = vocabularyItems
            .Select(item => new SearchCardItem(item.ToCardListItemResponse(), item.CreatedAt, item.UpdatedAt))
            .Concat(grammarItems.Select(item => new SearchCardItem(item.ToCardListItemResponse(), item.CreatedAt, item.UpdatedAt)))
            .Concat(kanjiItems.Select(item => new SearchCardItem(item.ToCardListItemResponse(), item.CreatedAt, item.UpdatedAt)))
            .OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt)
            .ToList();

        var pagedItems = merged
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => item.Item)
            .ToList();

        var meta = new MetaData
        {
            Page = page,
            PageSize = pageSize,
            Total = merged.Count,
        };

        return (pagedItems, meta);
    }

    public async Task<CardExplanationResponse> ExplainAsync(string cardId, ExplainCardRequest request)
    {
        var card = await _unitOfWork.Cards.GetExplainCardByIdAsync(cardId)
            ?? throw new ApplicationException(MessageConstants.CardMessage.NOT_FOUND);

        try
        {
            using var cts = new CancellationTokenSource(AiTimeout);
            var contentResult = await GenerateValidatedExplanationAsync(card, request.UserQuestion, cts.Token);

            return contentResult.Content.ToExplanationResponse(card, contentResult.Model);
        }
        catch (ApplicationException ex) when (IsAiProviderUnavailable(ex.Message))
        {
            throw new ApplicationException(MessageConstants.CardMessage.AI_EXPLANATION_UNAVAILABLE);
        }
        catch (OperationCanceledException)
        {
            throw new ApplicationException(MessageConstants.CardMessage.AI_EXPLANATION_UNAVAILABLE);
        }
    }

    private async Task<GenerationContentResult> GenerateValidatedExplanationAsync(
        Domain.Entities.Card card,
        string? userQuestion,
        CancellationToken cancellationToken)
    {
        var systemPrompt = CardExplanationPromptHelper.GetSystemPrompt();
        var userPrompt = CardExplanationPromptHelper.BuildUserPrompt(card, userQuestion);

        var initialResult = await _aiGenerationService.GenerateStructuredJsonAsync(systemPrompt, userPrompt, cancellationToken);
        var initialContent = TryParseContent(initialResult.Content);
        if (initialContent != null)
            return new GenerationContentResult(initialContent, initialResult.Model);

        _logger.LogWarning("AI card explanation JSON không hợp lệ cho card {CardId}, thử repair một lần", card.Id);

        var repairPrompt = CardExplanationPromptHelper.BuildRepairPrompt(card, userQuestion, initialResult.Content);
        var repairedResult = await _aiGenerationService.GenerateStructuredJsonAsync(systemPrompt, repairPrompt, cancellationToken);
        var repairedContent = TryParseContent(repairedResult.Content);

        if (repairedContent == null)
            throw new ApplicationException(MessageConstants.CardMessage.AI_EXPLANATION_INVALID);

        return new GenerationContentResult(repairedContent, repairedResult.Model);
    }

    private static CardExplanationContent? TryParseContent(string json)
    {
        try
        {
            return CardExplanationValidationHelper.ParseAndNormalize(json);
        }
        catch (ApplicationException ex) when (ex.Message == MessageConstants.CardMessage.AI_EXPLANATION_INVALID)
        {
            return null;
        }
    }

    private static bool IsAiProviderUnavailable(string message)
    {
        return message == MessageConstants.ExamSessionMessage.AI_ANALYSIS_UNAVAILABLE
            || message == MessageConstants.AiQuestionMessage.GENERATION_FAILED;
    }

    private async Task<(List<CardListItemResponse> Items, MetaData Meta)> SearchVocabularyOnlyAsync(
        string? query,
        string? level,
        int page,
        int pageSize)
    {
        var searchQuery = new VocabularySearchQuery
        {
            Q = query,
            Level = level,
            Status = PublishStatus.Published.ToString(),
            Page = page,
            PageSize = pageSize,
        };

        var (items, meta) = await _vocabularyDetailService.SearchAsync(searchQuery, string.Empty);
        return (items.Select(item => item.ToCardListItemResponse()).ToList(), meta);
    }

    private async Task<(List<CardListItemResponse> Items, MetaData Meta)> SearchGrammarOnlyAsync(
        string? query,
        string? level,
        int page,
        int pageSize)
    {
        var searchQuery = new GrammarSearchQuery
        {
            Q = query,
            Level = level,
            Status = PublishStatus.Published.ToString(),
            Page = page,
            PageSize = pageSize,
        };

        var (items, meta) = await _grammarService.SearchAsync(searchQuery, string.Empty);
        return (items.Select(item => item.ToCardListItemResponse()).ToList(), meta);
    }

    private async Task<(List<CardListItemResponse> Items, MetaData Meta)> SearchKanjiOnlyAsync(
        string? query,
        string? level,
        int page,
        int pageSize)
    {
        var searchQuery = new KanjiSearchQuery
        {
            Q = query,
            Level = level,
            Status = PublishStatus.Published.ToString(),
            Page = page,
            PageSize = pageSize,
        };

        var (items, meta) = await _kanjiService.SearchAsync(searchQuery, string.Empty);
        return (items.Select(item => item.ToCardListItemResponse()).ToList(), meta);
    }

    private async Task<List<VocabularyListItemResponse>> SearchAllVocabularyPublishedAsync(string? query, string? level)
    {
        const int batchSize = 100;
        var results = new List<VocabularyListItemResponse>();
        var currentPage = 1;
        var total = int.MaxValue;

        while (results.Count < total)
        {
            var searchQuery = new VocabularySearchQuery
            {
                Q = query,
                Level = level,
                Status = PublishStatus.Published.ToString(),
                Page = currentPage,
                PageSize = batchSize,
            };

            var (items, meta) = await _vocabularyDetailService.SearchAsync(searchQuery, string.Empty);
            total = meta.Total;

            if (items.Count == 0)
                break;

            results.AddRange(items);
            currentPage++;
        }

        return results;
    }

    private async Task<List<GrammarListItemResponse>> SearchAllGrammarPublishedAsync(string? query, string? level)
    {
        const int batchSize = 100;
        var results = new List<GrammarListItemResponse>();
        var currentPage = 1;
        var total = int.MaxValue;

        while (results.Count < total)
        {
            var searchQuery = new GrammarSearchQuery
            {
                Q = query,
                Level = level,
                Status = PublishStatus.Published.ToString(),
                Page = currentPage,
                PageSize = batchSize,
            };

            var (items, meta) = await _grammarService.SearchAsync(searchQuery, string.Empty);
            total = meta.Total;

            if (items.Count == 0)
                break;

            results.AddRange(items);
            currentPage++;
        }

        return results;
    }

    private async Task<List<KanjiListItemResponse>> SearchAllKanjiPublishedAsync(string? query, string? level)
    {
        const int batchSize = 100;
        var results = new List<KanjiListItemResponse>();
        var currentPage = 1;
        var total = int.MaxValue;

        while (results.Count < total)
        {
            var searchQuery = new KanjiSearchQuery
            {
                Q = query,
                Level = level,
                Status = PublishStatus.Published.ToString(),
                Page = currentPage,
                PageSize = batchSize,
            };

            var (items, meta) = await _kanjiService.SearchAsync(searchQuery, string.Empty);
            total = meta.Total;

            if (items.Count == 0)
                break;

            results.AddRange(items);
            currentPage++;
        }

        return results;
    }

    public async Task<(List<CardListItemResponse> Items, MetaData Meta)> SuggestByTopicAsync(TopicSuggestQuery query)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);
        var cardTypeEnum = EnumParsingHelper.ParseNullable<CardType>(query.CardType);
        var levelEnum = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);

        var keywords = ExtractKeywords(query.Topic);
        if (keywords.Count == 0)
            return (new List<CardListItemResponse>(), new MetaData { Page = page, PageSize = pageSize, Total = 0 });

        var (items, total) = await _unitOfWork.Cards.SuggestCardsByTopicAsync(
            keywords, cardTypeEnum, levelEnum, page, pageSize);

        var response = items.Select(c => c.ToCardListItemResponse()).ToList();

        var meta = new MetaData
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
        };

        return (response, meta);
    }

    private static List<string> ExtractKeywords(string topic)
    {
        return topic
            .Split(new[] { ' ', ',', ';', '、', '　' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim())
            .Where(k => k.Length >= 1)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private sealed record SearchCardItem(CardListItemResponse Item, DateTime CreatedAt, DateTime? UpdatedAt);
    private sealed record GenerationContentResult(CardExplanationContent Content, string Model);
}
