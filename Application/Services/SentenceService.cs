using Application.Common;
using Application.Helper;
using Application.Mappings;
using Application.DTOs.Sentences;
using Application.IRepositories;
using Application.IServices;
using Application.IServices.IInternal;
using Domain.Constants;
using Domain.Entities;

namespace Application.Services;

public class SentenceService : ISentenceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVoicevoxService _voicevoxService;

    public SentenceService(IUnitOfWork unitOfWork, IVoicevoxService voicevoxService)
    {
        _unitOfWork = unitOfWork;
        _voicevoxService = voicevoxService;
    }

    public async Task<SentenceResponse> CreateAsync(CreateSentenceRequest request, string currentUserId)
    {
        var text = request.Text.Trim();
        var shouldGenerateAudio = string.IsNullOrWhiteSpace(request.AudioUrl);
        var synthesisResult = shouldGenerateAudio
            ? await _voicevoxService.SynthesizeAsync(text, request.SpeakerId)
            : null;
        var audioUrl = shouldGenerateAudio ? synthesisResult?.AudioUrl : request.AudioUrl!.Trim();
        var speakerId = request.SpeakerId ?? synthesisResult?.SpeakerId;

        var sentence = new Sentence
        {
            Id = Guid.NewGuid().ToString(),
            Text = text,
            Meaning = request.Meaning.Trim(),
            AudioUrl = audioUrl,
            SpeakerId = speakerId,
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

        var text = request.Text.Trim();
        var shouldGenerateAudio = string.IsNullOrWhiteSpace(request.AudioUrl);
        var synthesisResult = shouldGenerateAudio
            ? await _voicevoxService.SynthesizeAsync(text, request.SpeakerId)
            : null;
        var audioUrl = shouldGenerateAudio ? synthesisResult?.AudioUrl : request.AudioUrl!.Trim();
        var speakerId = request.SpeakerId ?? synthesisResult?.SpeakerId ?? sentence.SpeakerId;

        sentence.Text = text;
        sentence.Meaning = request.Meaning.Trim();
        sentence.AudioUrl = audioUrl;
        sentence.SpeakerId = speakerId;
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
