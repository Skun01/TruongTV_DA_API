using Application.Common;
using Application.Helper;
using Application.Mappings;
using Application.DTOs.Sentences;
using Application.IRepositories;
using Application.IServices;
using Domain.Constants;
using Domain.Entities;

namespace Application.Services;

public class SentenceService : ISentenceService
{
    private readonly IUnitOfWork _unitOfWork;

    public SentenceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SentenceResponse> CreateAsync(CreateSentenceRequest request, string currentUserId)
    {
        var sentence = new Sentence
        {
            Id = Guid.NewGuid().ToString(),
            Text = request.Text.Trim(),
            Meaning = request.Meaning.Trim(),
            AudioUrl = string.IsNullOrWhiteSpace(request.AudioUrl) ? null : request.AudioUrl.Trim(),
            Level = EnumParsingHelper.ParseNullable<Domain.Enums.JlptLevel>(request.Level),
            CreatedBy = currentUserId,
        };

        await _unitOfWork.Sentences.AddAsync(sentence);
        await _unitOfWork.SaveChangesAsync();

        return sentence.ToResponse();
    }

    public async Task<SentenceResponse> GetByIdAsync(string id)
    {
        var sentence = await _unitOfWork.Sentences.GetByIdAsync(id);
        if (sentence == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        return sentence.ToResponse();
    }

    public async Task<(List<SentenceResponse> Items, MetaData Meta)> SearchAsync(SentenceSearchQuery query, string currentUserId)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);

        var levelEnum = EnumParsingHelper.ParseNullable<Domain.Enums.JlptLevel>(query.Level);
        var createdBy = query.CreatedByMe ? currentUserId : null;

        var (items, total) = await _unitOfWork.Sentences.SearchAsync(
            query.Q,
            levelEnum,
            createdBy,
            query.HasAudio,
            page,
            pageSize);

        var mappedItems = items.Select(item => item.ToResponse()).ToList();
        var meta = new MetaData
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
        };

        return (mappedItems, meta);
    }

    public async Task<SentenceResponse> UpdateAsync(string id, UpdateSentenceRequest request)
    {
        var sentence = await _unitOfWork.Sentences.GetByIdAsync(id);
        if (sentence == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        sentence.Text = request.Text.Trim();
        sentence.Meaning = request.Meaning.Trim();
        sentence.AudioUrl = string.IsNullOrWhiteSpace(request.AudioUrl) ? null : request.AudioUrl.Trim();
        sentence.Level = EnumParsingHelper.ParseNullable<Domain.Enums.JlptLevel>(request.Level);
        sentence.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Sentences.UpdateAsync(sentence);
        await _unitOfWork.SaveChangesAsync();

        return sentence.ToResponse();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var sentence = await _unitOfWork.Sentences.GetByIdAsync(id);
        if (sentence == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        _unitOfWork.Sentences.DeleteAsync(sentence);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
