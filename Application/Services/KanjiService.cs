using Application.Common;
using Application.DTOs.Kanji;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class KanjiService : IKanjiService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<KanjiService> _logger;

    public KanjiService(
        IUnitOfWork unitOfWork,
        ILogger<KanjiService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<KanjiDetailResponse> GetDetailAsync(string cardId, string? currentUserId)
    {
        var card = await _unitOfWork.Cards.GetKanjiDetailByIdAsync(cardId);
        if (card == null || card.KanjiDetail == null || card.CardType != CardType.Kanji)
            throw new AppException(MessageConstants.KanjiMessage.CARD_NOT_FOUND, 404);

        EnsureCardReadable(card, currentUserId);

        var radicals = await _unitOfWork.KanjiRadicals.GetByKanjiIdAsync(cardId);
        var notes = await _unitOfWork.UserCardNotes.GetByCardIdWithRelationsAsync(cardId);

        return card.ToKanjiDetailResponse(radicals, notes, currentUserId);
    }

    public async Task<(List<KanjiListItemResponse> Items, MetaData Meta)> SearchAsync(KanjiSearchQuery query, string currentUserId)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);
        var levelEnum = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);
        var statusEnum = EnumParsingHelper.ParseNullable<PublishStatus>(query.Status);
        var createdBy = query.CreatedByMe ? currentUserId : null;

        var (items, total) = await _unitOfWork.Cards.SearchKanjiAsync(
            query.Q,
            levelEnum,
            statusEnum,
            query.StrokeCountMin,
            query.StrokeCountMax,
            StringHelper.NormalizeOptional(query.Radical),
            createdBy,
            page,
            pageSize);

        var mapped = items.Select(item => item.ToKanjiListItemResponse()).ToList();
        var meta = new MetaData
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
        };

        return (mapped, meta);
    }

    public Task<ImportKanjiRequest> GetImportTemplateAsync()
    {
        return Task.FromResult(KanjiImportHelper.CreateTemplate());
    }

    public async Task<ImportKanjiRequest> ExportAsync(KanjiExportQuery query, string currentUserId)
    {
        var levelEnum = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);
        var statusEnum = EnumParsingHelper.ParseNullable<PublishStatus>(query.Status);
        var createdBy = query.CreatedByMe ? currentUserId : null;

        var cards = await _unitOfWork.Cards.GetKanjiExportAsync(
            query.Q,
            levelEnum,
            statusEnum,
            query.StrokeCountMin,
            query.StrokeCountMax,
            StringHelper.NormalizeOptional(query.Radical),
            createdBy);

        var resultItems = new List<ImportKanjiItemRequest>();

        foreach (var card in cards)
        {
            if (card.KanjiDetail == null || card.CardType != CardType.Kanji)
                continue;

            var radicals = await _unitOfWork.KanjiRadicals.GetByKanjiIdAsync(card.Id);
            resultItems.Add(card.ToImportItem(radicals));
        }

        return new ImportKanjiRequest
        {
            Items = resultItems,
        };
    }

    public async Task<KanjiImportPreviewResponse> PreviewImportAsync(ImportKanjiRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new AppException(MessageConstants.KanjiMessage.IMPORT_INVALID_PAYLOAD, 400);

        var previewItems = new List<KanjiImportPreviewItemResponse>();
        var batchKanjiSet = new HashSet<string>(StringComparer.Ordinal);

        for (var index = 0; index < request.Items.Count; index++)
        {
            var item = request.Items[index];
            var previewItem = new KanjiImportPreviewItemResponse
            {
                RowNumber = item.RowNumber.GetValueOrDefault(index + 1),
                Title = item.Title?.Trim() ?? string.Empty,
                Kanji = item.Kanji?.Trim() ?? string.Empty,
            };

            await KanjiImportHelper.ValidateImportItemAsync(_unitOfWork, item, previewItem, batchKanjiSet);
            previewItem.IsValid = previewItem.Errors.Count == 0;
            previewItems.Add(previewItem);
        }

        return new KanjiImportPreviewResponse
        {
            TotalItems = previewItems.Count,
            ValidItems = previewItems.Count(item => item.IsValid),
            InvalidItems = previewItems.Count(item => !item.IsValid),
            Items = previewItems,
        };
    }

    public async Task<KanjiImportCommitResponse> CommitImportAsync(ImportKanjiRequest request, string currentUserId)
    {
        var preview = await PreviewImportAsync(request);
        if (preview.InvalidItems > 0)
            return KanjiImportHelper.BuildBlockedCommitResponse(preview);

        var commitItems = new List<KanjiImportCommitItemResponse>();

        for (var index = 0; index < request.Items.Count; index++)
        {
            var item = request.Items[index];
            var commitItem = new KanjiImportCommitItemResponse
            {
                RowNumber = item.RowNumber.GetValueOrDefault(index + 1),
                Title = item.Title?.Trim() ?? string.Empty,
                Kanji = item.Kanji?.Trim() ?? string.Empty,
            };

            try
            {
                var result = await CreateAsync(item.ToCreateRequest(), currentUserId);
                commitItem.IsSuccess = true;
                commitItem.Action = "created";
                commitItem.CardId = result.Id;
            }
            catch (AppException ex)
            {
                commitItem.Errors.Add(ex.ErrorCode);
            }
            catch (ApplicationException ex)
            {
                commitItem.Errors.Add(ex.Message);
                _logger.LogError(
                    ex,
                    "Kanji import failed with application error at row {RowNumber}. Kanji: {Kanji}",
                    commitItem.RowNumber,
                    commitItem.Kanji);
            }
            catch (Exception ex)
            {
                commitItem.Errors.Add(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);
                _logger.LogError(
                    ex,
                    "Kanji import failed with unexpected error at row {RowNumber}. Kanji: {Kanji}",
                    commitItem.RowNumber,
                    commitItem.Kanji);
            }

            commitItems.Add(commitItem);
        }

        return new KanjiImportCommitResponse
        {
            TotalItems = commitItems.Count,
            SuccessfulItems = commitItems.Count(item => item.IsSuccess),
            FailedItems = commitItems.Count(item => !item.IsSuccess),
            HasValidationErrors = false,
            Items = commitItems,
        };
    }

    public async Task<KanjiDetailResponse> CreateAsync(CreateKanjiCardRequest request, string currentUserId)
    {
        var normalizedKanji = request.Kanji.Trim();
        if (await _unitOfWork.Cards.ExistsKanjiByCharacterAsync(normalizedKanji))
            throw new AppException(MessageConstants.KanjiMessage.KANJI_ALREADY_EXISTS, 409);

        var cardId = Guid.NewGuid().ToString();
        var card = new Card
        {
            Id = cardId,
            CardType = CardType.Kanji,
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level),
            Tags = StringHelper.NormalizeList(request.Tags),
            Status = EnumParsingHelper.ParseNullable<PublishStatus>(request.Status) ?? PublishStatus.Draft,
            CreatedBy = currentUserId,
        };

        card.KanjiDetail = new KanjiDetail
        {
            CardId = cardId,
            Kanji = normalizedKanji,
            StrokeCount = request.StrokeCount,
            StrokeOrderUrl = StringHelper.NormalizeOptional(request.StrokeOrderUrl),
            Onyomi = StringHelper.NormalizeList(request.Onyomi),
            Kunyomi = StringHelper.NormalizeList(request.Kunyomi),
            HanViet = StringHelper.NormalizeOptional(request.HanViet),
            MeaningVi = request.MeaningVi.Trim(),
        };

        await _unitOfWork.Cards.AddAsync(card);
        await SyncRadicalsAsync(cardId, request.Radicals);
        await _unitOfWork.SaveChangesAsync();
        await UpdateRadicalSelfLinkByCharacterAsync(normalizedKanji);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Cards.GetKanjiDetailByIdAsync(cardId)
            ?? throw new AppException(MessageConstants.KanjiMessage.CARD_NOT_FOUND, 404);

        return created.ToKanjiDetailResponse(
            await _unitOfWork.KanjiRadicals.GetByKanjiIdAsync(cardId),
            new List<UserCardNote>(),
            currentUserId);
    }

    public async Task<KanjiDetailResponse> UpdateAsync(string cardId, UpdateKanjiCardRequest request, string currentUserId)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId);
        if (card == null || card.CardType != CardType.Kanji)
            throw new AppException(MessageConstants.KanjiMessage.CARD_NOT_FOUND, 404);

        var detail = await _unitOfWork.KanjiDetails.GetByIdAsync(cardId);
        if (detail == null)
            throw new AppException(MessageConstants.KanjiMessage.DETAIL_NOT_FOUND, 404);

        var previousKanji = detail.Kanji;
        var normalizedKanji = request.Kanji.Trim();
        if (!string.Equals(detail.Kanji, normalizedKanji, StringComparison.Ordinal)
            && await _unitOfWork.Cards.ExistsKanjiByCharacterAsync(normalizedKanji))
            throw new AppException(MessageConstants.KanjiMessage.KANJI_ALREADY_EXISTS, 409);

        card.Title = request.Title.Trim();
        card.Summary = request.Summary.Trim();
        card.Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level);
        card.Tags = StringHelper.NormalizeList(request.Tags);
        card.Status = EnumParsingHelper.ParseNullable<PublishStatus>(request.Status) ?? card.Status;
        card.UpdatedAt = DateTime.UtcNow;

        detail.Kanji = normalizedKanji;
        detail.StrokeCount = request.StrokeCount;
        detail.StrokeOrderUrl = StringHelper.NormalizeOptional(request.StrokeOrderUrl);
        detail.Onyomi = StringHelper.NormalizeList(request.Onyomi);
        detail.Kunyomi = StringHelper.NormalizeList(request.Kunyomi);
        detail.HanViet = StringHelper.NormalizeOptional(request.HanViet);
        detail.MeaningVi = request.MeaningVi.Trim();

        await SyncRadicalsAsync(cardId, request.Radicals);

        _unitOfWork.Cards.UpdateAsync(card);
        _unitOfWork.KanjiDetails.UpdateAsync(detail);
        await _unitOfWork.SaveChangesAsync();
        await UpdateRadicalSelfLinkByCharacterAsync(previousKanji);
        await UpdateRadicalSelfLinkByCharacterAsync(normalizedKanji);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Cards.GetKanjiDetailByIdAsync(cardId)
            ?? throw new AppException(MessageConstants.KanjiMessage.CARD_NOT_FOUND, 404);

        var radicals = await _unitOfWork.KanjiRadicals.GetByKanjiIdAsync(cardId);
        var notes = await _unitOfWork.UserCardNotes.GetByCardIdWithRelationsAsync(cardId);
        return updated.ToKanjiDetailResponse(radicals, notes, currentUserId);
    }

    public async Task<bool> SoftDeleteAsync(string cardId, string currentUserId)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId);
        if (card == null || card.CardType != CardType.Kanji)
            throw new AppException(MessageConstants.KanjiMessage.CARD_NOT_FOUND, 404);

        card.Status = PublishStatus.Archived;
        card.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Cards.UpdateAsync(card);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static void EnsureCardReadable(Card card, string? currentUserId)
    {
        if (card.Status != PublishStatus.Published && (string.IsNullOrWhiteSpace(currentUserId) || card.CreatedBy != currentUserId))
            throw new AppException(MessageConstants.KanjiMessage.READ_FORBIDDEN, 401);
    }

    private async Task SyncRadicalsAsync(string kanjiId, List<KanjiRadicalUpsertRequest> radicals)
    {
        var existingLinks = await _unitOfWork.KanjiRadicals.GetByKanjiIdAsync(kanjiId);
        var keptRadicalIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var request in radicals)
        {
            var character = request.Character.Trim();
            var radical = await _unitOfWork.RadicalDetails.GetByCharacterAsync(character);
            if (radical == null)
            {
                radical = new RadicalDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    Character = character,
                    MeaningVi = request.MeaningVi.Trim(),
                };

                await _unitOfWork.RadicalDetails.AddAsync(radical);
            }
            else
            {
                radical.MeaningVi = request.MeaningVi.Trim();
                _unitOfWork.RadicalDetails.UpdateAsync(radical);
            }

            await SyncRadicalKanjiLinkAsync(radical);

            if (!keptRadicalIds.Add(radical.Id))
                continue;

            if (existingLinks.Any(link => link.RadicalId == radical.Id))
                continue;

            await _unitOfWork.KanjiRadicals.AddAsync(new KanjiRadical
            {
                KanjiId = kanjiId,
                RadicalId = radical.Id,
            });
        }

        foreach (var link in existingLinks.Where(link => !keptRadicalIds.Contains(link.RadicalId)))
        {
            _unitOfWork.KanjiRadicals.DeleteAsync(link);
        }
    }

    private async Task SyncRadicalKanjiLinkAsync(RadicalDetail radical)
    {
        var linkedCard = await FindKanjiCardByCharacterAsync(radical.Character);
        radical.KanjiCardId = linkedCard?.Id;
    }

    private async Task UpdateRadicalSelfLinkByCharacterAsync(string character)
    {
        if (string.IsNullOrWhiteSpace(character))
            return;

        var radical = await _unitOfWork.RadicalDetails.GetByCharacterAsync(character);
        if (radical == null)
            return;

        await SyncRadicalKanjiLinkAsync(radical);
        _unitOfWork.RadicalDetails.UpdateAsync(radical);
    }

    private async Task<Card?> FindKanjiCardByCharacterAsync(string character)
    {
        var searchQuery = new KanjiSearchQuery
        {
            Q = character,
            Page = 1,
            PageSize = 200,
        };

        var (items, _) = await SearchAsync(searchQuery, string.Empty);
        var match = items.FirstOrDefault(item => string.Equals(item.Kanji, character, StringComparison.Ordinal));
        if (match == null)
            return null;

        return await _unitOfWork.Cards.GetByIdAsync(match.Id);
    }
}
