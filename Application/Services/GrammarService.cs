using Application.Common;
using Application.DTOs.Grammar;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class GrammarService : IGrammarService
{
    private readonly IUnitOfWork _unitOfWork;

    public GrammarService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GrammarDetailResponse> GetDetailAsync(string cardId, string? currentUserId)
    {
        var card = await _unitOfWork.Cards.GetGrammarDetailByIdAsync(cardId);
        if (card == null || card.GrammarDetail == null || card.CardType != CardType.Grammar)
            throw new AppException(MessageConstants.GrammarMessage.CARD_NOT_FOUND, 404);

        EnsureCardReadable(card, currentUserId);

        var relations = await _unitOfWork.GrammarRelations.GetByGrammarIdAsync(cardId);
        var resources = await _unitOfWork.GrammarResources.GetByCardIdAsync(cardId);
        var notes = await _unitOfWork.UserCardNotes.GetByCardIdWithRelationsAsync(cardId);

        return card.ToGrammarDetailResponse(relations, resources, notes, currentUserId);
    }

    public async Task<(List<GrammarListItemResponse> Items, MetaData Meta)> SearchAsync(GrammarSearchQuery query, string currentUserId)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);

        var levelEnum = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);
        var statusEnum = EnumParsingHelper.ParseNullable<PublishStatus>(query.Status);
        var registerEnum = EnumParsingHelper.ParseNullable<RegisterType>(query.Register);
        var createdBy = query.CreatedByMe ? currentUserId : null;

        var (items, total) = await _unitOfWork.Cards.SearchGrammarAsync(
            query.Q,
            levelEnum,
            statusEnum,
            registerEnum,
            createdBy,
            page,
            pageSize);

        var mapped = items.Select(item => item.ToGrammarListItemResponse()).ToList();
        var meta = new MetaData
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
        };

        return (mapped, meta);
    }

    public async Task<GrammarDetailResponse> CreateAsync(CreateGrammarCardRequest request, string currentUserId)
    {
        var cardId = Guid.NewGuid().ToString();
        var card = new Card
        {
            Id = cardId,
            CardType = CardType.Grammar,
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level),
            Tags = StringHelper.NormalizeList(request.Tags),
            Status = EnumParsingHelper.ParseNullable<PublishStatus>(request.Status) ?? PublishStatus.Draft,
            CreatedBy = currentUserId,
        };

        card.GrammarDetail = new GrammarDetail
        {
            CardId = cardId,
            Structures = NormalizeStructures(request.Structures),
            Explanation = GrammarMarkdownHelper.NormalizeOptional(request.Explanation, "explanation", 10000),
            Caution = GrammarMarkdownHelper.NormalizeOptional(request.Caution, "caution", 5000),
            Register = EnumParsingHelper.ParseNullable<RegisterType>(request.Register),
            AlternateForms = StringHelper.NormalizeList(request.AlternateForms),
        };

        await ValidateAndSyncRelationsAsync(cardId, request.Relations);
        await SyncResourcesAsync(cardId, request.Resources);

        await _unitOfWork.Cards.AddAsync(card);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Cards.GetGrammarDetailByIdAsync(cardId)
            ?? throw new AppException(MessageConstants.GrammarMessage.CARD_NOT_FOUND, 404);

        return created.ToGrammarDetailResponse(
            await _unitOfWork.GrammarRelations.GetByGrammarIdAsync(cardId),
            await _unitOfWork.GrammarResources.GetByCardIdAsync(cardId),
            new List<UserCardNote>(),
            currentUserId);
    }

    public async Task<GrammarDetailResponse> UpdateAsync(string cardId, UpdateGrammarCardRequest request, string currentUserId)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId);
        if (card == null || card.CardType != CardType.Grammar)
            throw new AppException(MessageConstants.GrammarMessage.CARD_NOT_FOUND, 404);

        var detail = await _unitOfWork.GrammarDetails.GetByIdAsync(cardId);
        if (detail == null)
            throw new AppException(MessageConstants.GrammarMessage.DETAIL_NOT_FOUND, 404);

        card.Title = request.Title.Trim();
        card.Summary = request.Summary.Trim();
        card.Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level);
        card.Tags = StringHelper.NormalizeList(request.Tags);
        card.Status = EnumParsingHelper.ParseNullable<PublishStatus>(request.Status) ?? card.Status;
        card.UpdatedAt = DateTime.UtcNow;

        detail.Structures = NormalizeStructures(request.Structures);
        detail.Explanation = GrammarMarkdownHelper.NormalizeOptional(request.Explanation, "explanation", 10000);
        detail.Caution = GrammarMarkdownHelper.NormalizeOptional(request.Caution, "caution", 5000);
        detail.Register = EnumParsingHelper.ParseNullable<RegisterType>(request.Register);
        detail.AlternateForms = StringHelper.NormalizeList(request.AlternateForms);

        await ValidateAndSyncRelationsAsync(cardId, request.Relations);
        await SyncResourcesAsync(cardId, request.Resources);

        _unitOfWork.Cards.UpdateAsync(card);
        _unitOfWork.GrammarDetails.UpdateAsync(detail);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Cards.GetGrammarDetailByIdAsync(cardId)
            ?? throw new AppException(MessageConstants.GrammarMessage.CARD_NOT_FOUND, 404);

        var relations = await _unitOfWork.GrammarRelations.GetByGrammarIdAsync(cardId);
        var resources = await _unitOfWork.GrammarResources.GetByCardIdAsync(cardId);
        var notes = await _unitOfWork.UserCardNotes.GetByCardIdWithRelationsAsync(cardId);
        return updated.ToGrammarDetailResponse(relations, resources, notes, currentUserId);
    }

    public async Task<bool> SoftDeleteAsync(string cardId, string currentUserId)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId);
        if (card == null || card.CardType != CardType.Grammar)
            throw new AppException(MessageConstants.GrammarMessage.CARD_NOT_FOUND, 404);

        card.Status = PublishStatus.Archived;
        card.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Cards.UpdateAsync(card);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static void EnsureCardReadable(Card card, string? currentUserId)
    {
        if (card.Status != PublishStatus.Published && (string.IsNullOrWhiteSpace(currentUserId) || card.CreatedBy != currentUserId))
            throw new AppException(MessageConstants.GrammarMessage.READ_FORBIDDEN, 401);
    }

    private static List<Domain.ValueObjects.GrammarStructureItem> NormalizeStructures(List<GrammarStructureRequest> structures)
    {
        var result = new List<Domain.ValueObjects.GrammarStructureItem>();

        for (var i = 0; i < structures.Count; i++)
        {
            var structure = structures[i];
            var normalizedPattern = GrammarMarkdownHelper.NormalizeRequired(
                structure.Pattern,
                $"structures[{i}].pattern",
                1000);

            Dictionary<string, string>? normalizedAnnotations = null;
            if (structure.Annotations != null && structure.Annotations.Count > 0)
            {
                normalizedAnnotations = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var annotation in structure.Annotations)
                {
                    var key = annotation.Key.Trim();
                    if (string.IsNullOrWhiteSpace(key))
                        throw new AppException(
                            MessageConstants.GrammarMessage.INVALID_RICH_TEXT,
                            400,
                            details: new { field = $"structures[{i}].annotations", reason = "Annotation key is required." });

                    var normalizedValue = GrammarMarkdownHelper.NormalizeRequired(
                        annotation.Value,
                        $"structures[{i}].annotations.{key}",
                        1000);

                    normalizedAnnotations[key] = normalizedValue;
                }
            }

            result.Add(new Domain.ValueObjects.GrammarStructureItem
            {
                Pattern = normalizedPattern,
                Annotations = normalizedAnnotations,
            });
        }

        return result;
    }

    private async Task ValidateAndSyncRelationsAsync(string grammarId, List<GrammarRelationUpsertRequest> relations)
    {
        var existing = await _unitOfWork.GrammarRelations.GetByGrammarIdAsync(grammarId);
        foreach (var relation in existing)
        {
            _unitOfWork.GrammarRelations.DeleteAsync(relation);
        }

        var uniqueSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var relation in relations)
        {
            var relatedId = relation.RelatedId.Trim();
            if (string.IsNullOrWhiteSpace(relatedId) || relatedId == grammarId)
                throw new AppException(MessageConstants.GrammarMessage.INVALID_RELATION, 400);

            var relationType = EnumParsingHelper.ParseNullable<GrammarRelationType>(relation.RelationType);
            if (!relationType.HasValue)
                throw new AppException(MessageConstants.GrammarMessage.INVALID_RELATION, 400);

            var relatedCard = await _unitOfWork.Cards.GetByIdAsync(relatedId);
            if (relatedCard == null || relatedCard.CardType != CardType.Grammar)
                throw new AppException(MessageConstants.GrammarMessage.RELATED_CARD_NOT_FOUND, 404);

            var dedupKey = $"{relatedId}:{relationType.Value}";
            if (!uniqueSet.Add(dedupKey))
                continue;

            await _unitOfWork.GrammarRelations.AddAsync(new GrammarRelation
            {
                GrammarId = grammarId,
                RelatedId = relatedId,
                RelationType = relationType.Value,
            });
        }
    }

    private async Task SyncResourcesAsync(string grammarId, List<GrammarResourceUpsertRequest> resources)
    {
        var existing = await _unitOfWork.GrammarResources.GetByCardIdAsync(grammarId);
        foreach (var resource in existing)
        {
            _unitOfWork.GrammarResources.DeleteAsync(resource);
        }

        foreach (var resource in resources)
        {
            await _unitOfWork.GrammarResources.AddAsync(new GrammarResource
            {
                Id = Guid.NewGuid().ToString(),
                CardId = grammarId,
                Title = resource.Title.Trim(),
                Url = resource.Url.Trim(),
            });
        }
    }
}
