using Application.Common;
using Application.DTOs.Sentences;
using Application.IRepositories;
using Application.IServices;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class SentenceService : ISentenceService
{
    private readonly IUnitOfWork _unitOfWork;

    public SentenceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SentenceResponse> CreateAsync(CreateSentenceRequest request)
    {
        var sentence = new Sentence
        {
            Id = Guid.NewGuid().ToString(),
            Text = request.Text.Trim(),
            Meaning = request.Meaning.Trim(),
            AudioUrl = string.IsNullOrWhiteSpace(request.AudioUrl) ? null : request.AudioUrl.Trim(),
            Level = ParseLevel(request.Level),
        };

        await _unitOfWork.Sentences.AddAsync(sentence);
        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(sentence);
    }

    public async Task<SentenceResponse> GetByIdAsync(string id)
    {
        var sentence = await _unitOfWork.Sentences.GetByIdAsync(id);
        if (sentence == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        return MapToResponse(sentence);
    }

    public async Task<(List<SentenceResponse> Items, MetaData Meta)> SearchAsync(string? q, string? level, int page, int pageSize)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        var levelEnum = ParseLevel(level);

        var (items, total) = await _unitOfWork.Sentences.SearchAsync(q, levelEnum, page, pageSize);

        var mappedItems = items.Select(MapToResponse).ToList();
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
        sentence.Level = ParseLevel(request.Level);
        sentence.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Sentences.UpdateAsync(sentence);
        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(sentence);
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

    private static SentenceResponse MapToResponse(Sentence sentence)
    {
        return new SentenceResponse
        {
            Id = sentence.Id,
            Text = sentence.Text,
            Meaning = sentence.Meaning,
            AudioUrl = sentence.AudioUrl,
            Level = sentence.Level?.ToString(),
            CreatedAt = sentence.CreatedAt,
            UpdatedAt = sentence.UpdatedAt,
        };
    }

    private static JlptLevel? ParseLevel(string? level)
    {
        if (string.IsNullOrWhiteSpace(level))
            return null;

        if (Enum.TryParse<JlptLevel>(level.Trim(), true, out var parsed))
            return parsed;

        throw new ApplicationException(MessageConstants.CommonMessage.INVALID);
    }
}
