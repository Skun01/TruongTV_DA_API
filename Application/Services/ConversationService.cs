using Application.Common;
using Application.DTOs.Conversations;
using Application.DTOs.Conversations.Queries;
using Application.Helper;
using Application.IServices;
using Application.IServices.IInternal;
using Application.IRepositories;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class ConversationService : IConversationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiConversationService _aiConversationService;

    public ConversationService(IUnitOfWork unitOfWork, IAiConversationService aiConversationService)
    {
        _unitOfWork = unitOfWork;
        _aiConversationService = aiConversationService;
    }

    public ScenarioListResponse GetScenarios()
    {
        return ConversationMappings.GetScenarios();
    }

    public async Task<ConversationDetailResponse> GetConversationAsync(
        string conversationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var session = await GetOwnedConversationAsync(conversationId, userId);
        var orderedMessages = OrderMessages(session);
        var scenarioText = ConversationPromptHelper.ResolveScenarioText(session.Scenario, session.CustomScenario);

        return session.ToDetailResponse(orderedMessages, scenarioText);
    }

    public async Task<StartConversationResponse> StartConversationAsync(
        StartConversationRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTime.UtcNow;
        var conversationId = Guid.NewGuid().ToString();
        var scenarioText = ConversationPromptHelper.ResolveScenarioText(request.Scenario, request.CustomScenario);

        var generation = await _aiConversationService.GenerateConversationJsonAsync(
            ConversationPromptHelper.GetSystemPrompt(),
            ConversationPromptHelper.BuildStartPrompt(scenarioText, request.Level),
            cancellationToken);

        var aiContent = ConversationAiResponseParser.ParseMessage(generation.Content);

        var session = new ConversationSession
        {
            Id = conversationId,
            UserId = userId,
            Scenario = request.Scenario,
            CustomScenario = request.CustomScenario,
            Level = request.Level,
            Status = ConversationSessionStatus.Active,
            StartedAt = startedAt,
            TotalMessages = 1,
            UserMessagesCount = 0,
            Score = 0
        };

        var aiMessage = CreateAiMessage(conversationId, aiContent, startedAt);

        await _unitOfWork.ConversationSessions.AddAsync(session);
        await _unitOfWork.ConversationMessages.AddAsync(aiMessage);
        await _unitOfWork.SaveChangesAsync();

        return aiMessage.ToStartConversationResponse(conversationId);
    }

    public async Task<SendMessageResponse> SendMessageAsync(
        string conversationId,
        SendMessageRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var session = await GetOwnedConversationAsync(conversationId, userId);

        if (session.Status == ConversationSessionStatus.Completed)
            throw new ApplicationException(MessageConstants.ConversationMessage.ALREADY_COMPLETED);

        var orderedMessages = OrderMessages(session);
        var scenarioText = ConversationPromptHelper.ResolveScenarioText(session.Scenario, session.CustomScenario);

        var generation = await _aiConversationService.GenerateConversationJsonAsync(
            ConversationPromptHelper.GetSystemPrompt(),
            ConversationPromptHelper.BuildContinuePrompt(
                scenarioText,
                session.Level,
                ConversationPromptHelper.BuildConversationHistory(orderedMessages),
                request.UserMessage),
            cancellationToken);

        var aiContent = ConversationAiResponseParser.ParseMessage(generation.Content);
        var createdAt = DateTime.UtcNow;

        var userMessage = new ConversationMessage
        {
            Id = Guid.NewGuid().ToString(),
            ConversationId = session.Id,
            Sender = MessageSender.User,
            Text = request.UserMessage,
            CreatedAt = createdAt
        };

        var aiMessage = CreateAiMessage(session.Id, aiContent, createdAt.AddMilliseconds(1));

        await _unitOfWork.ConversationMessages.AddAsync(userMessage);
        await _unitOfWork.ConversationMessages.AddAsync(aiMessage);

        session.TotalMessages += 2;
        session.UserMessagesCount++;

        await _unitOfWork.SaveChangesAsync();

        var newWordsLearned = orderedMessages
            .Append(aiMessage)
            .CountLearnedWords();

        return aiMessage.ToSendMessageResponse(session.TotalMessages, session.UserMessagesCount, newWordsLearned);
    }

    public async Task<ConversationResultResponse> CompleteConversationAsync(
        string conversationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var session = await GetOwnedConversationAsync(conversationId, userId);
        var orderedMessages = OrderMessages(session);

        if (session.Status == ConversationSessionStatus.Completed
            && !string.IsNullOrWhiteSpace(session.Feedback))
        {
            return BuildResultResponse(session, orderedMessages);
        }

        session.Status = ConversationSessionStatus.Completed;
        session.CompletedAt ??= DateTime.UtcNow;

        var engagementScore = CalculateEngagementScore(session.TotalMessages, orderedMessages);

        try
        {
            var scenarioText = ConversationPromptHelper.ResolveScenarioText(session.Scenario, session.CustomScenario);
            var generation = await _aiConversationService.GenerateConversationJsonAsync(
                ConversationPromptHelper.GetSystemPrompt(),
                ConversationPromptHelper.BuildEndPrompt(
                    scenarioText,
                    session.Level,
                    ConversationPromptHelper.BuildConversationHistory(orderedMessages),
                    session.TotalMessages,
                    session.UserMessagesCount),
                cancellationToken);

            var evaluation = ConversationAiResponseParser.ParseEvaluation(generation.Content);
            session.Feedback = evaluation.Feedback;
            session.Score = Math.Clamp((engagementScore + evaluation.Score) / 2, 0, 100);
            session.ResultModel = generation.Model;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            session.Feedback = $"Bạn đã hoàn thành cuộc hội thoại với {session.TotalMessages} tin nhắn.";
            session.Score = engagementScore;
            session.ResultModel = null;
        }

        session.FeedbackGeneratedAt = DateTime.UtcNow;
        session.ResultPromptVersion = ConversationPromptHelper.PromptVersion;

        await _unitOfWork.SaveChangesAsync();

        return BuildResultResponse(session, orderedMessages);
    }

    public Task<ConversationResultResponse> GetResultAsync(
        string conversationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return CompleteConversationAsync(conversationId, userId, cancellationToken);
    }

    public async Task<(List<ConversationListItemResponse> Items, MetaData Meta)> SearchHistoryAsync(
        ConversationHistoryQuery query,
        string userId)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);

        var items = await _unitOfWork.ConversationSessions.GetByUserIdAsync(userId, page, pageSize);
        var totalCount = await _unitOfWork.ConversationSessions.CountByUserIdAsync(userId);

        var responseItems = items.Select(x => x.ToListItemResponse()).ToList();

        return (responseItems, new MetaData
        {
            Page = page,
            PageSize = pageSize,
            Total = totalCount
        });
    }

    public async Task DeleteConversationAsync(string conversationId, string userId)
    {
        var session = await _unitOfWork.ConversationSessions.GetByIdAsync(conversationId);

        if (session == null || session.UserId != userId)
            throw new ApplicationException(MessageConstants.ConversationMessage.NOT_FOUND);

        _unitOfWork.ConversationSessions.DeleteAsync(session);
        await _unitOfWork.SaveChangesAsync();
    }

    private ConversationResultResponse BuildResultResponse(ConversationSession session, List<ConversationMessage> orderedMessages)
    {
        var duration = ConversationDurationHelper.Format(session.StartedAt, session.CompletedAt);
        return session.ToResultResponse(orderedMessages, duration);
    }

    private async Task<ConversationSession> GetOwnedConversationAsync(string conversationId, string userId)
    {
        var session = await _unitOfWork.ConversationSessions.GetByIdWithMessagesAsync(conversationId);

        if (session == null || session.UserId != userId)
            throw new ApplicationException(MessageConstants.ConversationMessage.NOT_FOUND);

        return session;
    }

    private static List<ConversationMessage> OrderMessages(ConversationSession session)
    {
        return session.Messages
            .OrderBy(x => x.CreatedAt)
            .ToList();
    }

    private static ConversationMessage CreateAiMessage(
        string conversationId,
        ConversationAiMessageContent aiContent,
        DateTime createdAt)
    {
        return new ConversationMessage
        {
            Id = Guid.NewGuid().ToString(),
            ConversationId = conversationId,
            Sender = MessageSender.AI,
            Text = aiContent.Text,
            Suggestions = aiContent.Suggestions,
            NewVocabulary = aiContent.NewVocabulary
                .Select(x => new ExtractedVocabulary
                {
                    Id = Guid.NewGuid().ToString(),
                    Word = x.Word,
                    Reading = x.Reading,
                    Meaning = x.Meaning,
                    Example = x.Example,
                    JlptLevel = ParseJlptLevel(x.JlptLevel)
                })
                .ToList(),
            GrammarPoints = aiContent.GrammarPoints,
            CreatedAt = createdAt
        };
    }

    private static int CalculateEngagementScore(int totalMessages, IEnumerable<ConversationMessage> messages)
    {
        var score = 50;

        if (totalMessages >= 5)
            score += 10;

        if (totalMessages >= 10)
            score += 10;

        var newWords = messages.CountLearnedWords();
        score += Math.Min(newWords * 10, 30);

        return Math.Min(score, 100);
    }

    private static JlptLevel ParseJlptLevel(string? level)
    {
        return level?.ToUpperInvariant() switch
        {
            "N1" => JlptLevel.N1,
            "N2" => JlptLevel.N2,
            "N3" => JlptLevel.N3,
            "N4" => JlptLevel.N4,
            "N5" => JlptLevel.N5,
            _ => JlptLevel.N5
        };
    }
}
