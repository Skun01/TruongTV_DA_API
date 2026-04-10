using Application.Common;
using Application.DTOs.Cards;
using Application.DTOs.Grammar;
using Application.DTOs.Vocabulary;
using Application.Helper;
using Application.IServices;
using Application.Mappings;
using Domain.Enums;

namespace Application.Services;

public class CardService : ICardService
{
    private readonly IVocabularyDetailService _vocabularyDetailService;
    private readonly IGrammarService _grammarService;

    public CardService(
        IVocabularyDetailService vocabularyDetailService,
        IGrammarService grammarService)
    {
        _vocabularyDetailService = vocabularyDetailService;
        _grammarService = grammarService;
    }

    public async Task<(List<CardListItemResponse> Items, MetaData Meta)> SearchAsync(CardSearchQuery query)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);
        var cardTypeEnum = EnumParsingHelper.ParseNullable<CardType>(query.CardType);

        if (cardTypeEnum == CardType.Vocab)
            return await SearchVocabularyOnlyAsync(query.Q, query.Level, page, pageSize);

        if (cardTypeEnum == CardType.Grammar)
            return await SearchGrammarOnlyAsync(query.Q, query.Level, page, pageSize);

        var vocabularyItems = await SearchAllVocabularyPublishedAsync(query.Q, query.Level);
        var grammarItems = await SearchAllGrammarPublishedAsync(query.Q, query.Level);

        var merged = vocabularyItems
            .Select(item => new SearchCardItem(item.ToCardListItemResponse(), item.CreatedAt, item.UpdatedAt))
            .Concat(grammarItems.Select(item => new SearchCardItem(item.ToCardListItemResponse(), item.CreatedAt, item.UpdatedAt)))
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

    private sealed record SearchCardItem(CardListItemResponse Item, DateTime CreatedAt, DateTime? UpdatedAt);
}
