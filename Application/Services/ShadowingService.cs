using Application.Common;
using Application.DTOs.Internal;
using Application.DTOs.Shadowing;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.IServices.IInternal;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class ShadowingService : IShadowingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileUploadService _fileUploadService;
    private readonly IPronunciationAssessmentService _pronunciationAssessmentService;

    public ShadowingService(
        IUnitOfWork unitOfWork,
        IFileUploadService fileUploadService,
        IPronunciationAssessmentService pronunciationAssessmentService)
    {
        _unitOfWork = unitOfWork;
        _fileUploadService = fileUploadService;
        _pronunciationAssessmentService = pronunciationAssessmentService;
    }

    public async Task<(List<ShadowingTopicListItemResponse> Items, MetaData Meta)> SearchTopicsAsync(ShadowingTopicListQuery query, string userId)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);
        var level = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);
        var visibility = EnumParsingHelper.ParseNullable<DeckVisibility>(query.Visibility);

        var (items, total) = await _unitOfWork.ShadowingTopics.SearchReadableAsync(
            userId,
            query.Q,
            level,
            visibility,
            query.OfficialOnly,
            page,
            pageSize);

        return (
            items.Select(x => x.ToTopicListItemResponse(userId)).ToList(),
            new MetaData
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
            });
    }

    public async Task<ShadowingTopicDetailResponse> GetTopicDetailAsync(string topicId, string userId)
    {
        var topic = await _unitOfWork.ShadowingTopics.GetReadableDetailByIdAsync(topicId, userId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.TOPIC_NOT_FOUND);

        return topic.ToTopicDetailResponse(userId);
    }

    public async Task<ShadowingAttemptResponse> SubmitAttemptAsync(string userId, SubmitShadowingAttemptRequest request, CancellationToken cancellationToken = default)
    {
        if (request.AudioBytes.Length == 0)
            throw new AppException(MessageConstants.ShadowingMessage.INVALID_AUDIO, 400);

        var topic = await _unitOfWork.ShadowingTopics.GetReadableDetailByIdAsync(request.TopicId, userId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.TOPIC_NOT_FOUND);

        var topicSentence = topic.TopicSentences.FirstOrDefault(x => x.SentenceId == request.SentenceId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.SENTENCE_NOT_ATTACHED);

        PronunciationAssessmentResult assessment;

        await using (var assessmentStream = new MemoryStream(request.AudioBytes, writable: false))
        {
            assessment = await _pronunciationAssessmentService.AssessAsync(
                assessmentStream,
                request.ContentType,
                topicSentence.Sentence.Text,
                request.Locale,
                cancellationToken);
        }

        var uploadResult = await UploadAttemptAudioAsync(userId, request, cancellationToken);

        var mediaAsset = new MediaAsset
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            FileUrl = uploadResult.FileUrl,
            StorageKey = uploadResult.StorageKey,
            OriginalFileName = request.FileName,
            ContentType = uploadResult.ContentType,
            SizeInBytes = request.SizeInBytes,
            FileType = uploadResult.FileType,
            UsageType = uploadResult.UsageType,
            StorageProvider = StorageProvider.Cloud,
        };
        await _unitOfWork.MediaAssets.AddAsync(mediaAsset);

        var attempt = new ShadowingAttempt
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            TopicId = topic.Id,
            SentenceId = request.SentenceId,
            AudioAssetId = mediaAsset.Id,
            Locale = request.Locale,
            RecognizedText = assessment.RecognizedText,
            PronScore = assessment.PronScore,
            AccuracyScore = assessment.AccuracyScore,
            FluencyScore = assessment.FluencyScore,
            CompletenessScore = assessment.CompletenessScore,
            ProsodyScore = assessment.ProsodyScore,
            ErrorTypes = assessment.ErrorTypes.Count == 0 ? null : string.Join(",", assessment.ErrorTypes),
            DurationMs = assessment.DurationMs,
            RawResultJson = assessment.RawJson,
        };

        await _unitOfWork.ShadowingAttempts.AddAsync(attempt);
        await _unitOfWork.SaveChangesAsync();

        attempt.Topic = topic;
        attempt.Sentence = topicSentence.Sentence;
        attempt.AudioAsset = mediaAsset;

        return attempt.ToAttemptResponse();
    }

    public async Task<(List<ShadowingAttemptHistoryItemResponse> Items, MetaData Meta)> GetAttemptHistoryAsync(ShadowingAttemptHistoryQuery query, string userId)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);

        var (items, total) = await _unitOfWork.ShadowingAttempts.SearchByUserAsync(
            userId,
            query.TopicId,
            query.SentenceId,
            page,
            pageSize);

        return (
            items.Select(x => x.ToAttemptHistoryItemResponse()).ToList(),
            new MetaData
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
            });
    }

    public async Task<ShadowingSentenceProgressResponse> GetSentenceProgressAsync(string sentenceId, string userId)
    {
        var sentence = await _unitOfWork.Sentences.GetByIdAsync(sentenceId);
        if (sentence == null)
            throw new ApplicationException(MessageConstants.ShadowingMessage.SENTENCE_NOT_FOUND);

        var attempts = await _unitOfWork.ShadowingAttempts.GetByUserAndSentenceAsync(userId, sentenceId);
        if (attempts.Count == 0)
        {
            return new ShadowingSentenceProgressResponse
            {
                SentenceId = sentenceId,
            };
        }

        var ordered = attempts
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        return new ShadowingSentenceProgressResponse
        {
            SentenceId = sentenceId,
            AttemptsCount = ordered.Count,
            LatestPronScore = ordered.First().PronScore,
            BestPronScore = ordered
                .Where(x => x.PronScore.HasValue)
                .Select(x => x.PronScore!.Value)
                .DefaultIfEmpty()
                .Max(),
            LastAttemptAt = ordered.First().CreatedAt,
        };
    }

    private async Task<FileUploadResult> UploadAttemptAudioAsync(
        string userId,
        SubmitShadowingAttemptRequest request,
        CancellationToken cancellationToken)
    {
        await using var uploadStream = new MemoryStream(request.AudioBytes, writable: false);
        return await _fileUploadService.UploadAsync(new FileUploadRequest
        {
            UserId = userId,
            FileName = request.FileName,
            ContentType = request.ContentType,
            Content = uploadStream,
            FileType = FileType.Audio,
            UsageType = ResourceUsageType.Audio,
        }, cancellationToken);
    }
}
