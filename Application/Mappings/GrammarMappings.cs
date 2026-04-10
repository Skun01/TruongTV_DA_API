using Application.DTOs.CardNotes;
using Application.DTOs.Grammar;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Mappings;

public static class GrammarMappings
{
    public static ImportGrammarItemRequest ToImportItem(
        this Card card,
        List<GrammarRelation> relations,
        List<GrammarResource> resources)
    {
        return new ImportGrammarItemRequest
        {
            Title = card.Title,
            Summary = card.Summary,
            Level = card.Level?.ToString(),
            Tags = card.Tags,
            Status = card.Status.ToString(),
            Structures = card.GrammarDetail?.Structures.Select(s => new GrammarStructureRequest
            {
                Pattern = s.Pattern,
                Annotations = s.Annotations,
            }).ToList() ?? new List<GrammarStructureRequest>(),
            Explanation = card.GrammarDetail?.Explanation,
            Caution = card.GrammarDetail?.Caution,
            Register = card.GrammarDetail?.Register?.ToString(),
            AlternateForms = card.GrammarDetail?.AlternateForms ?? new List<string>(),
            Relations = relations.Select(r => new GrammarRelationUpsertRequest
            {
                RelatedId = r.RelatedId,
                RelationType = r.RelationType.ToString(),
            }).ToList(),
            Resources = resources.Select(r => new GrammarResourceUpsertRequest
            {
                Title = r.Title,
                Url = r.Url,
            }).ToList(),
        };
    }

    public static CreateGrammarCardRequest ToCreateRequest(this ImportGrammarItemRequest item)
    {
        return new CreateGrammarCardRequest
        {
            Title = item.Title,
            Summary = item.Summary,
            Level = item.Level,
            Tags = item.Tags,
            Status = item.Status,
            Structures = item.Structures,
            Explanation = item.Explanation,
            Caution = item.Caution,
            Register = item.Register,
            AlternateForms = item.AlternateForms,
            Relations = item.Relations,
            Resources = item.Resources,
        };
    }

    public static GrammarDetailResponse ToGrammarDetailResponse(
        this Card card,
        List<GrammarRelation> relations,
        List<GrammarResource> resources,
        List<UserCardNote> notes,
        string? currentUserId)
    {
        return new GrammarDetailResponse
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
            Structures = card.GrammarDetail?.Structures.Select(s => s.ToStructureResponse()).ToList() ?? new List<GrammarStructureResponse>(),
            Explanation = card.GrammarDetail?.Explanation,
            Caution = card.GrammarDetail?.Caution,
            Register = card.GrammarDetail?.Register?.ToString(),
            AlternateForms = card.GrammarDetail?.AlternateForms ?? new List<string>(),
            Relations = relations.Select(r => new GrammarRelationResponse
            {
                RelatedId = r.RelatedId,
                RelationType = r.RelationType.ToString(),
            }).ToList(),
            Resources = resources.Select(r => new GrammarResourceResponse
            {
                Id = r.Id,
                Title = r.Title,
                Url = r.Url,
            }).ToList(),
            UserNotes = notes.Select(n => n.ToCardNoteResponse(currentUserId ?? string.Empty)).ToList(),
        };
    }

    public static GrammarListItemResponse ToGrammarListItemResponse(this Card card)
    {
        return new GrammarListItemResponse
        {
            Id = card.Id,
            Title = card.Title,
            Summary = card.Summary,
            Level = card.Level?.ToString(),
            Tags = card.Tags,
            Status = card.Status.ToString(),
            CreatedAt = card.CreatedAt,
            UpdatedAt = card.UpdatedAt,
            Register = card.GrammarDetail?.Register?.ToString(),
            StructuresCount = card.GrammarDetail?.Structures.Count ?? 0,
            AlternateForms = card.GrammarDetail?.AlternateForms ?? new List<string>(),
        };
    }

    public static GrammarStructureItem ToStructureItem(this GrammarStructureRequest request)
    {
        return new GrammarStructureItem
        {
            Pattern = request.Pattern.Trim(),
            Annotations = request.Annotations,
        };
    }

    public static GrammarStructureResponse ToStructureResponse(this GrammarStructureItem item)
    {
        return new GrammarStructureResponse
        {
            Pattern = item.Pattern,
            Annotations = item.Annotations,
        };
    }
}
