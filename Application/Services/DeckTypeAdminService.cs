using Application.Common;
using Application.DTOs.Decks;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;

namespace Application.Services;

public class DeckTypeAdminService : IDeckTypeAdminService
{
    private readonly IUnitOfWork _unitOfWork;

    public DeckTypeAdminService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(List<AdminDeckTypeResponse> Items, MetaData Meta)> SearchAsync(AdminDeckTypeListQuery query)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);
        var (items, total) = await _unitOfWork.DeckTypes.SearchAsync(query.Q, page, pageSize);

        return (
            items.Select(x => x.ToAdminResponse()).ToList(),
            new MetaData
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
            });
    }

    public async Task<AdminDeckTypeResponse> GetDetailAsync(string id)
    {
        var deckType = await _unitOfWork.DeckTypes.GetByIdAsync(id);
        if (deckType == null)
            throw new ApplicationException(MessageConstants.DeckTypeMessage.NOT_FOUND);

        return deckType.ToAdminResponse();
    }

    public async Task<AdminDeckTypeResponse> CreateAsync(CreateDeckTypeRequest request)
    {
        var normalizedName = request.Name.Trim();
        if (await _unitOfWork.DeckTypes.ExistsByNameAsync(normalizedName))
            throw new ApplicationException(MessageConstants.DeckTypeMessage.NAME_EXISTS);

        var deckType = new DeckType
        {
            Id = Guid.NewGuid().ToString(),
            Name = normalizedName,
        };

        await _unitOfWork.DeckTypes.AddAsync(deckType);
        await _unitOfWork.SaveChangesAsync();

        return deckType.ToAdminResponse();
    }

    public async Task<AdminDeckTypeResponse> UpdateAsync(string id, UpdateDeckTypeRequest request)
    {
        var deckType = await _unitOfWork.DeckTypes.GetByIdAsync(id);
        if (deckType == null)
            throw new ApplicationException(MessageConstants.DeckTypeMessage.NOT_FOUND);

        var normalizedName = request.Name.Trim();
        if (await _unitOfWork.DeckTypes.ExistsByNameAsync(normalizedName, id))
            throw new ApplicationException(MessageConstants.DeckTypeMessage.NAME_EXISTS);

        deckType.Name = normalizedName;
        _unitOfWork.DeckTypes.UpdateAsync(deckType);
        await _unitOfWork.SaveChangesAsync();

        return deckType.ToAdminResponse();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var deckType = await _unitOfWork.DeckTypes.GetByIdAsync(id);
        if (deckType == null)
            throw new ApplicationException(MessageConstants.DeckTypeMessage.NOT_FOUND);

        if (await _unitOfWork.Decks.ExistsByTypeIdAsync(id))
            throw new ApplicationException(MessageConstants.DeckTypeMessage.IN_USE);

        _unitOfWork.DeckTypes.DeleteAsync(deckType);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
