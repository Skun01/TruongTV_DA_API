using Application.Common;
using Application.DTOs.Grammar;

namespace Application.IServices;

public interface IGrammarService
{
    Task<GrammarDetailResponse> GetDetailAsync(string cardId, string? currentUserId);
    Task<(List<GrammarListItemResponse> Items, MetaData Meta)> SearchAsync(GrammarSearchQuery query, string currentUserId);
    Task<GrammarDetailResponse> CreateAsync(CreateGrammarCardRequest request, string currentUserId);
    Task<GrammarDetailResponse> UpdateAsync(string cardId, UpdateGrammarCardRequest request, string currentUserId);
    Task<bool> SoftDeleteAsync(string cardId, string currentUserId);
}
