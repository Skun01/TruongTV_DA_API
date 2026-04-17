using Application.Common;
using Application.DTOs.Decks;

namespace Application.IServices;

public interface IDeckTypeAdminService
{
    Task<(List<AdminDeckTypeResponse> Items, MetaData Meta)> SearchAsync(AdminDeckTypeListQuery query);
    Task<AdminDeckTypeResponse> GetDetailAsync(string id);
    Task<AdminDeckTypeResponse> CreateAsync(CreateDeckTypeRequest request);
    Task<AdminDeckTypeResponse> UpdateAsync(string id, UpdateDeckTypeRequest request);
    Task<bool> DeleteAsync(string id);
}
