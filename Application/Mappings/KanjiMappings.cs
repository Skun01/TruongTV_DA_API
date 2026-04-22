using Application.DTOs.CardNotes;
using Application.DTOs.Kanji;
using Domain.Entities;

namespace Application.Mappings;

public static class KanjiMappings
{
    public static ImportKanjiItemRequest ToImportItem(this Card card, List<KanjiRadical> radicals)
    {
        return new ImportKanjiItemRequest
        {
            Title = card.Title,
            Summary = card.Summary,
            Level = card.Level?.ToString(),
            Tags = card.Tags,
            Status = card.Status.ToString(),
            Kanji = card.KanjiDetail?.Kanji ?? string.Empty,
            StrokeCount = card.KanjiDetail?.StrokeCount ?? 0,
            StrokeOrderUrl = card.KanjiDetail?.StrokeOrderUrl,
            Onyomi = card.KanjiDetail?.Onyomi ?? new List<string>(),
            Kunyomi = card.KanjiDetail?.Kunyomi ?? new List<string>(),
            HanViet = card.KanjiDetail?.HanViet,
            MeaningVi = card.KanjiDetail?.MeaningVi ?? string.Empty,
            Radicals = radicals.Select(r => new KanjiRadicalUpsertRequest
            {
                Character = r.Radical.Character,
                MeaningVi = r.Radical.MeaningVi,
            }).ToList(),
        };
    }

    public static CreateKanjiCardRequest ToCreateRequest(this ImportKanjiItemRequest item)
    {
        return new CreateKanjiCardRequest
        {
            Title = item.Title,
            Summary = item.Summary,
            Level = item.Level,
            Tags = item.Tags,
            Status = string.IsNullOrWhiteSpace(item.Status) ? "Published" : item.Status,
            Kanji = item.Kanji,
            StrokeCount = item.StrokeCount,
            StrokeOrderUrl = item.StrokeOrderUrl,
            Onyomi = item.Onyomi,
            Kunyomi = item.Kunyomi,
            HanViet = item.HanViet,
            MeaningVi = item.MeaningVi,
            Radicals = item.Radicals,
        };
    }

    public static KanjiDetailResponse ToKanjiDetailResponse(
        this Card card,
        List<KanjiRadical> radicals,
        List<UserCardNote> notes,
        string? currentUserId)
    {
        return new KanjiDetailResponse
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
            Kanji = card.KanjiDetail?.Kanji ?? string.Empty,
            StrokeCount = card.KanjiDetail?.StrokeCount ?? 0,
            StrokeOrderUrl = card.KanjiDetail?.StrokeOrderUrl,
            Onyomi = card.KanjiDetail?.Onyomi ?? new List<string>(),
            Kunyomi = card.KanjiDetail?.Kunyomi ?? new List<string>(),
            HanViet = card.KanjiDetail?.HanViet,
            MeaningVi = card.KanjiDetail?.MeaningVi ?? string.Empty,
            Radicals = radicals.Select(r => new KanjiRadicalResponse
            {
                Id = r.Radical.Id,
                Character = r.Radical.Character,
                MeaningVi = r.Radical.MeaningVi,
                KanjiCardId = r.Radical.KanjiCardId,
            }).ToList(),
            UserNotes = notes.Select(n => n.ToCardNoteResponse(currentUserId ?? string.Empty)).ToList(),
        };
    }

    public static KanjiListItemResponse ToKanjiListItemResponse(this Card card)
    {
        return new KanjiListItemResponse
        {
            Id = card.Id,
            Title = card.Title,
            Summary = card.Summary,
            Level = card.Level?.ToString(),
            Tags = card.Tags,
            Status = card.Status.ToString(),
            CreatedAt = card.CreatedAt,
            UpdatedAt = card.UpdatedAt,
            Kanji = card.KanjiDetail?.Kanji ?? string.Empty,
            StrokeCount = card.KanjiDetail?.StrokeCount ?? 0,
            HanViet = card.KanjiDetail?.HanViet,
            MeaningVi = card.KanjiDetail?.MeaningVi ?? string.Empty,
            RadicalCount = card.KanjiRadicals.Count,
        };
    }
}
