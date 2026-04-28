using Application.Common;
using Application.DTOs.Shadowing;
using Application.DTOs.ShadowingAdmin;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class AdminShadowingService : IAdminShadowingService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminShadowingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(List<ShadowingTopicListItemResponse> Items, MetaData Meta)> SearchTopicsAsync(AdminShadowingTopicListQuery query)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);
        var level = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);
        var visibility = EnumParsingHelper.ParseNullable<DeckVisibility>(query.Visibility);
        var status = EnumParsingHelper.ParseNullable<PublishStatus>(query.Status);

        var (items, total) = await _unitOfWork.ShadowingTopics.SearchAdminAsync(
            query.Q,
            level,
            visibility,
            status,
            query.IsOfficial,
            query.CreatedBy,
            page,
            pageSize);

        return (
            items.Select(x => x.ToTopicListItemResponse(null)).ToList(),
            new MetaData
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
            });
    }

    public async Task<ShadowingTopicDetailResponse> GetTopicDetailAsync(string topicId)
    {
        var topic = await _unitOfWork.ShadowingTopics.GetAdminDetailByIdAsync(topicId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.TOPIC_NOT_FOUND);

        return topic.ToTopicDetailResponse(null);
    }

    public async Task<ShadowingTopicDetailResponse> CreateTopicAsync(CreateShadowingTopicRequest request, string currentUserId)
    {
        var topic = new ShadowingTopic
        {
            Id = Guid.NewGuid().ToString(),
            CreatedBy = currentUserId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            CoverImageUrl = StringHelper.NormalizeOptional(request.CoverImageUrl),
            Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level),
            Visibility = EnumParsingHelper.ParseNullable<DeckVisibility>(request.Visibility) ?? DeckVisibility.Public,
            Status = EnumParsingHelper.ParseNullable<PublishStatus>(request.Status) ?? PublishStatus.Draft,
            IsOfficial = true,
            SentencesCount = 0,
        };

        await _unitOfWork.ShadowingTopics.AddAsync(topic);
        await _unitOfWork.SaveChangesAsync();

        return await GetTopicDetailAsync(topic.Id);
    }

    public async Task<ShadowingTopicDetailResponse> UpdateTopicAsync(string topicId, UpdateShadowingTopicRequest request)
    {
        var topic = await _unitOfWork.ShadowingTopics.GetByIdAsync(topicId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.TOPIC_NOT_FOUND);

        if (request.Title != null)
            topic.Title = request.Title.Trim();

        if (request.Description != null)
            topic.Description = request.Description.Trim();

        if (request.CoverImageUrl != null)
            topic.CoverImageUrl = StringHelper.NormalizeOptional(request.CoverImageUrl);

        if (request.Level != null)
            topic.Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level);

        if (request.Visibility != null)
            topic.Visibility = EnumParsingHelper.ParseNullable<DeckVisibility>(request.Visibility) ?? topic.Visibility;

        if (request.Status != null)
            topic.Status = EnumParsingHelper.ParseNullable<PublishStatus>(request.Status) ?? topic.Status;

        topic.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.ShadowingTopics.UpdateAsync(topic);
        await _unitOfWork.SaveChangesAsync();

        return await GetTopicDetailAsync(topicId);
    }

    public async Task<(List<AdminShadowingAvailableSentenceResponse> Items, MetaData Meta)> SearchAvailableSentencesAsync(
        string topicId,
        AdminShadowingAvailableSentenceQuery query,
        string currentUserId)
    {
        _ = currentUserId;

        var topic = await _unitOfWork.ShadowingTopics.GetByIdAsync(topicId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.TOPIC_NOT_FOUND);

        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);
        var level = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);
        var (sentences, total) = await _unitOfWork.Sentences.SearchAsync(
            query.Q,
            level,
            null,
            query.HasAudio,
            page,
            pageSize);

        var attachedMap = (await _unitOfWork.ShadowingTopicSentences.GetByTopicIdAsync(topic.Id))
            .ToDictionary(x => x.SentenceId, StringComparer.Ordinal);

        return (
            sentences.Select(x => x.ToAvailableSentenceResponse(attachedMap.GetValueOrDefault(x.Id))).ToList(),
            new MetaData
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
            });
    }

    public async Task<bool> DeleteTopicAsync(string topicId)
    {
        var topic = await _unitOfWork.ShadowingTopics.GetByIdAsync(topicId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.TOPIC_NOT_FOUND);

        _unitOfWork.ShadowingTopics.DeleteAsync(topic);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<ShadowingTopicSentenceResponse> AttachSentenceAsync(string topicId, AttachShadowingTopicSentenceRequest request)
    {
        var topic = await _unitOfWork.ShadowingTopics.GetByIdAsync(topicId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.TOPIC_NOT_FOUND);

        var sentence = await _unitOfWork.Sentences.GetByIdAsync(request.SentenceId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.SENTENCE_NOT_FOUND);

        var existing = await _unitOfWork.ShadowingTopicSentences.GetByTopicAndSentenceIdAsync(topicId, request.SentenceId);
        if (existing != null)
            throw new ApplicationException(MessageConstants.ShadowingMessage.SENTENCE_ALREADY_ATTACHED);

        await _unitOfWork.ShadowingTopicSentences.AddAsync(new ShadowingTopicSentence
        {
            TopicId = topicId,
            SentenceId = request.SentenceId,
            Position = request.Position,
            Note = StringHelper.NormalizeOptional(request.Note),
        });

        topic.SentencesCount += 1;
        topic.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.ShadowingTopics.UpdateAsync(topic);
        await _unitOfWork.SaveChangesAsync();

        return new ShadowingTopicSentence
        {
            TopicId = topicId,
            SentenceId = request.SentenceId,
            Position = request.Position,
            Note = StringHelper.NormalizeOptional(request.Note),
            Sentence = sentence,
        }.ToTopicSentenceResponse();
    }

    public async Task<List<ShadowingTopicSentenceResponse>> BulkAttachSentencesAsync(string topicId, BulkAttachShadowingTopicSentencesRequest request)
    {
        var topic = await _unitOfWork.ShadowingTopics.GetByIdAsync(topicId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.TOPIC_NOT_FOUND);

        var duplicatedPosition = request.Items
            .GroupBy(x => x.Position)
            .Any(group => group.Count() > 1);
        if (duplicatedPosition)
            throw new ApplicationException(MessageConstants.ShadowingMessage.DUPLICATE_POSITION);

        var duplicatedSentence = request.Items
            .GroupBy(x => x.SentenceId, StringComparer.Ordinal)
            .Any(group => group.Count() > 1);
        if (duplicatedSentence)
            throw new ApplicationException(MessageConstants.ShadowingMessage.SENTENCE_ALREADY_ATTACHED);

        var existingLinks = await _unitOfWork.ShadowingTopicSentences.GetByTopicIdAsync(topicId);
        var existingSentenceIds = existingLinks
            .Select(x => x.SentenceId)
            .ToHashSet(StringComparer.Ordinal);

        var responses = new List<ShadowingTopicSentenceResponse>();
        foreach (var item in request.Items)
        {
            if (existingSentenceIds.Contains(item.SentenceId))
                throw new ApplicationException(MessageConstants.ShadowingMessage.SENTENCE_ALREADY_ATTACHED);

            var sentence = await _unitOfWork.Sentences.GetByIdAsync(item.SentenceId)
                ?? throw new ApplicationException(MessageConstants.ShadowingMessage.SENTENCE_NOT_FOUND);

            var link = new ShadowingTopicSentence
            {
                TopicId = topicId,
                SentenceId = item.SentenceId,
                Position = item.Position,
                Note = StringHelper.NormalizeOptional(item.Note),
            };

            await _unitOfWork.ShadowingTopicSentences.AddAsync(link);
            existingSentenceIds.Add(item.SentenceId);

            responses.Add(new ShadowingTopicSentence
            {
                TopicId = topicId,
                SentenceId = item.SentenceId,
                Position = item.Position,
                Note = link.Note,
                Sentence = sentence,
            }.ToTopicSentenceResponse());
        }

        topic.SentencesCount += request.Items.Count;
        topic.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.ShadowingTopics.UpdateAsync(topic);
        await _unitOfWork.SaveChangesAsync();

        return responses
            .OrderBy(x => x.Position)
            .ToList();
    }

    public async Task<ShadowingTopicSentenceResponse> UpdateTopicSentenceAsync(string topicId, string sentenceId, UpdateShadowingTopicSentenceRequest request)
    {
        var topic = await _unitOfWork.ShadowingTopics.GetByIdAsync(topicId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.TOPIC_NOT_FOUND);

        var link = await _unitOfWork.ShadowingTopicSentences.GetByTopicAndSentenceIdAsync(topicId, sentenceId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.SENTENCE_NOT_ATTACHED);

        link.Position = request.Position;
        link.Note = StringHelper.NormalizeOptional(request.Note);
        _unitOfWork.ShadowingTopicSentences.UpdateAsync(link);

        topic.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.ShadowingTopics.UpdateAsync(topic);
        await _unitOfWork.SaveChangesAsync();

        var updatedTopic = await _unitOfWork.ShadowingTopics.GetAdminDetailByIdAsync(topicId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.TOPIC_NOT_FOUND);

        return updatedTopic.TopicSentences
            .Where(x => x.Sentence != null)
            .First(x => x.SentenceId == sentenceId)
            .ToTopicSentenceResponse();
    }

    public async Task<bool> DeleteTopicSentenceAsync(string topicId, string sentenceId)
    {
        var topic = await _unitOfWork.ShadowingTopics.GetByIdAsync(topicId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.TOPIC_NOT_FOUND);

        var link = await _unitOfWork.ShadowingTopicSentences.GetByTopicAndSentenceIdAsync(topicId, sentenceId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.SENTENCE_NOT_ATTACHED);

        _unitOfWork.ShadowingTopicSentences.DeleteAsync(link);
        topic.SentencesCount = Math.Max(0, topic.SentencesCount - 1);
        topic.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.ShadowingTopics.UpdateAsync(topic);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<List<ShadowingTopicSentenceResponse>> ReorderTopicSentencesAsync(string topicId, ReorderShadowingTopicSentencesRequest request)
    {
        var topic = await _unitOfWork.ShadowingTopics.GetByIdAsync(topicId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.TOPIC_NOT_FOUND);

        var duplicatedPosition = request.Items
            .GroupBy(x => x.Position)
            .Any(g => g.Count() > 1);
        if (duplicatedPosition)
            throw new ApplicationException(MessageConstants.ShadowingMessage.DUPLICATE_POSITION);

        var links = await _unitOfWork.ShadowingTopicSentences.GetByTopicIdAsync(topicId);
        var map = links.ToDictionary(x => x.SentenceId, StringComparer.Ordinal);

        foreach (var item in request.Items)
        {
            if (!map.TryGetValue(item.SentenceId, out var link))
                throw new ApplicationException(MessageConstants.ShadowingMessage.SENTENCE_NOT_ATTACHED);

            link.Position = item.Position;
            _unitOfWork.ShadowingTopicSentences.UpdateAsync(link);
        }

        topic.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.ShadowingTopics.UpdateAsync(topic);
        await _unitOfWork.SaveChangesAsync();

        var updatedTopic = await _unitOfWork.ShadowingTopics.GetAdminDetailByIdAsync(topicId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.TOPIC_NOT_FOUND);

        return updatedTopic.TopicSentences
            .Where(x => x.Sentence != null)
            .OrderBy(x => x.Position)
            .Select(x => x.ToTopicSentenceResponse())
            .ToList();
    }

    public async Task<ShadowingTopicAnalyticsResponse> GetTopicAnalyticsAsync(string topicId)
    {
        var topic = await _unitOfWork.ShadowingTopics.GetByIdAsync(topicId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.TOPIC_NOT_FOUND);

        var attemptsCount = await _unitOfWork.ShadowingAttempts.CountByTopicAsync(topicId);
        var distinctUsersCount = await _unitOfWork.ShadowingAttempts.CountDistinctUsersByTopicAsync(topicId);
        var averagePronScore = await _unitOfWork.ShadowingAttempts.GetAveragePronScoreByTopicAsync(topicId);
        var latestAttemptAt = await _unitOfWork.ShadowingAttempts.GetLatestAttemptAtByTopicAsync(topicId);

        return new ShadowingTopicAnalyticsResponse
        {
            TopicId = topic.Id,
            AttemptsCount = attemptsCount,
            DistinctUsersCount = distinctUsersCount,
            AveragePronScore = averagePronScore,
            LatestAttemptAt = latestAttemptAt,
        };
    }

    public async Task<List<ShadowingTopicSentenceAnalyticsResponse>> GetTopicSentenceAnalyticsAsync(string topicId)
    {
        var topic = await _unitOfWork.ShadowingTopics.GetAdminDetailByIdAsync(topicId)
            ?? throw new ApplicationException(MessageConstants.ShadowingMessage.TOPIC_NOT_FOUND);

        var analyticsMap = await _unitOfWork.ShadowingAttempts.GetSentenceAnalyticsByTopicAsync(topicId);

        return topic.TopicSentences
            .Where(x => x.Sentence != null)
            .OrderBy(x => x.Position)
            .Select(x =>
            {
                analyticsMap.TryGetValue(x.SentenceId, out var analytics);
                return x.ToSentenceAnalyticsResponse(
                    analytics.AttemptsCount,
                    analytics.DistinctUsersCount,
                    analytics.AveragePronScore,
                    analytics.LatestAttemptAt);
            })
            .ToList();
    }
}
