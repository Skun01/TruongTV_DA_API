using System.Text.Json;
using Application.Common;
using Application.DTOs.Common;
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

    public async Task<StartConversationResponse> StartConversationAsync(
        StartConversationRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var session = new ConversationSession
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Scenario = request.Scenario,
            CustomScenario = request.CustomScenario,
            Level = request.Level,
            Status = ConversationSessionStatus.Active,
            StartedAt = DateTime.UtcNow,
            TotalMessages = 0,
            UserMessagesCount = 0,
            Score = 0
        };

        await _unitOfWork.ConversationSessions.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        var systemPrompt = ConversationPromptHelper.GetSystemPrompt();
        var userPrompt = ConversationPromptHelper.BuildStartPrompt(request.Scenario, request.Level, request.CustomScenario);

        var jsonResponse = await _aiConversationService.GenerateConversationJsonAsync(systemPrompt, userPrompt, cancellationToken);
        var aiData = ParseAiResponse(jsonResponse);

        var aiMessage = new ConversationMessage
        {
            Id = Guid.NewGuid().ToString(),
            ConversationId = session.Id,
            Sender = MessageSender.AI,
            Text = aiData.Text,
            Suggestions = aiData.Suggestions,
            NewVocabulary = aiData.NewVocabulary.Select(v => new ExtractedVocabulary
            {
                Id = Guid.NewGuid().ToString(),
                MessageId = "",
                Word = v.Word,
                Reading = v.Reading,
                Meaning = v.Meaning,
                Example = v.Example,
                JlptLevel = ParseJlptLevel(v.JlptLevel)
            }).ToList(),
            GrammarPoints = aiData.GrammarPoints,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ConversationMessages.AddAsync(aiMessage);
        session.TotalMessages = 1;
        await _unitOfWork.SaveChangesAsync();

        return new StartConversationResponse
        {
            ConversationId = session.Id,
            AiMessage = new AiMessageDto
            {
                Text = aiMessage.Text,
                Suggestions = aiMessage.Suggestions ?? new List<string>()
            }
        };
    }

    public async Task<SendMessageResponse> SendMessageAsync(
        string conversationId,
        SendMessageRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.ConversationSessions.GetByIdWithMessagesAsync(conversationId);

        if (session == null || session.UserId != userId)
            throw new ApplicationException(MessageConstants.UserMessage.NOT_FOUND);

        if (session.Status == ConversationSessionStatus.Completed)
            throw new ApplicationException(MessageConstants.CommonMessage.INVALID);

        var userMessage = new ConversationMessage
        {
            Id = Guid.NewGuid().ToString(),
            ConversationId = session.Id,
            Sender = MessageSender.User,
            Text = request.UserMessage,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ConversationMessages.AddAsync(userMessage);
        session.TotalMessages++;
        session.UserMessagesCount++;

        var history = string.Join("\n", session.Messages.Select(m =>
            $"{(m.Sender == MessageSender.User ? "User" : "AI")}: {m.Text}"));

        var systemPrompt = ConversationPromptHelper.GetSystemPrompt();
        var userPrompt = ConversationPromptHelper.BuildContinuePrompt(
            session.Scenario,
            session.Level,
            history,
            request.UserMessage);

        var jsonResponse = await _aiConversationService.GenerateConversationJsonAsync(systemPrompt, userPrompt, cancellationToken);
        var aiData = ParseAiResponse(jsonResponse);

        var aiMessage = new ConversationMessage
        {
            Id = Guid.NewGuid().ToString(),
            ConversationId = session.Id,
            Sender = MessageSender.AI,
            Text = aiData.Text,
            Suggestions = aiData.Suggestions,
            NewVocabulary = aiData.NewVocabulary.Select(v => new ExtractedVocabulary
            {
                Id = Guid.NewGuid().ToString(),
                MessageId = "",
                Word = v.Word,
                Reading = v.Reading,
                Meaning = v.Meaning,
                Example = v.Example,
                JlptLevel = ParseJlptLevel(v.JlptLevel)
            }).ToList(),
            GrammarPoints = aiData.GrammarPoints,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ConversationMessages.AddAsync(aiMessage);
        session.TotalMessages++;

        await _unitOfWork.SaveChangesAsync();

        var totalNewWords = session.Messages
            .SelectMany(m => m.NewVocabulary)
            .Count(v => !string.IsNullOrWhiteSpace(v.Word));

        return new SendMessageResponse
        {
            AiMessage = new SendAiMessageDto
            {
                Text = aiMessage.Text,
                Suggestions = aiMessage.Suggestions ?? new List<string>(),
                NewVocabulary = aiMessage.NewVocabulary.Select(v => new VocabularyItemDto
                {
                    Word = v.Word,
                    Reading = v.Reading,
                    Meaning = v.Meaning,
                    Example = v.Example
                }).ToList(),
                GrammarPoints = aiMessage.GrammarPoints
            },
            Summary = new ConversationSummaryDto
            {
                TotalMessages = session.TotalMessages,
                UserMessagesCount = session.UserMessagesCount,
                NewWordsLearned = totalNewWords
            }
        };
    }

    public async Task<ConversationResultResponse> GetResultAsync(
        string conversationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.ConversationSessions.GetByIdWithMessagesAsync(conversationId);

        if (session == null || session.UserId != userId)
            throw new ApplicationException(MessageConstants.UserMessage.NOT_FOUND);

        if (session.Status == ConversationSessionStatus.Active)
        {
            session.Status = ConversationSessionStatus.Completed;
            session.CompletedAt = DateTime.UtcNow;
            session.Score = CalculateScore(session);
            await _unitOfWork.SaveChangesAsync();
        }

        var history = string.Join("\n", session.Messages.Select(m =>
            $"{(m.Sender == MessageSender.User ? "User" : "AI")}: {m.Text}"));

        var systemPrompt = ConversationPromptHelper.GetSystemPrompt();
        var userPrompt = ConversationPromptHelper.BuildEndPrompt(
            session.Scenario,
            session.Level,
            history,
            session.TotalMessages,
            session.UserMessagesCount);

        string feedback;
        int score;
        try
        {
            var jsonResponse = await _aiConversationService.GenerateConversationJsonAsync(systemPrompt, userPrompt, cancellationToken);
            var endData = ParseEndResponse(jsonResponse);
            feedback = endData.Feedback;
            score = endData.Score;
            session.Score = (session.Score + score) / 2;
        }
        catch
        {
            feedback = $"Bạn đã hoàn thành cuộc hội thoại với {session.TotalMessages} tin nhắn.";
            score = session.Score;
        }

        await _unitOfWork.SaveChangesAsync();

        var duration = session.CompletedAt.HasValue
            ? $"{(session.CompletedAt.Value - session.StartedAt).Minutes}m"
            : "0m";

        return session.ToResultResponse(session.Messages.ToList(), feedback, duration);
    }

    public async Task<(List<ConversationListItemResponse> Items, MetaData Meta)> SearchHistoryAsync(
        ConversationHistoryQuery query,
        string userId)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);

        var items = await _unitOfWork.ConversationSessions.GetByUserIdAsync(userId, page, pageSize);
        var totalCount = await _unitOfWork.ConversationSessions.CountByUserIdAsync(userId);

        var responseItems = items.Select(s => s.ToListItemResponse()).ToList();

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
            throw new ApplicationException(MessageConstants.UserMessage.NOT_FOUND);

        _unitOfWork.ConversationSessions.DeleteAsync(session);
        await _unitOfWork.SaveChangesAsync();
    }

    private static int CalculateScore(ConversationSession session)
    {
        var score = 50;

        if (session.TotalMessages >= 5) score += 10;
        if (session.TotalMessages >= 10) score += 10;

        var newWords = session.Messages
            .SelectMany(m => m.NewVocabulary)
            .Count(v => !string.IsNullOrWhiteSpace(v.Word));
        score += Math.Min(newWords * 10, 30);

        return Math.Min(score, 100);
    }

    private static ConversationAiResponse ParseAiResponse(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var text = root.GetProperty("text").GetString() ?? "";
            var suggestions = new List<string>();
            if (root.TryGetProperty("suggestions", out var sugElement) && sugElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var s in sugElement.EnumerateArray())
                    suggestions.Add(s.GetString() ?? "");
            }

            var vocabulary = new List<VocabularyResponse>();
            if (root.TryGetProperty("newVocabulary", out var vocabElement) && vocabElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var v in vocabElement.EnumerateArray())
                {
                    vocabulary.Add(new VocabularyResponse
                    {
                        Word = v.GetProperty("word").GetString() ?? "",
                        Reading = v.TryGetProperty("reading", out var r) ? r.GetString() ?? "" : "",
                        Meaning = v.GetProperty("meaning").GetString() ?? "",
                        Example = v.TryGetProperty("example", out var e) ? e.GetString() ?? "" : "",
                        JlptLevel = v.TryGetProperty("jlptLevel", out var l) ? l.GetString() ?? "N5" : "N5"
                    });
                }
            }

            var grammarPoints = new List<string>();
            if (root.TryGetProperty("grammarPoints", out var gramElement) && gramElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var g in gramElement.EnumerateArray())
                    grammarPoints.Add(g.GetString() ?? "");
            }

            return new ConversationAiResponse
            {
                Text = text,
                Suggestions = suggestions,
                NewVocabulary = vocabulary,
                GrammarPoints = grammarPoints
            };
        }
        catch
        {
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);
        }
    }

    private static EndConversationResponse ParseEndResponse(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new EndConversationResponse
            {
                Feedback = root.GetProperty("feedback").GetString() ?? "",
                Score = root.TryGetProperty("score", out var s) ? s.GetInt32() : 50
            };
        }
        catch
        {
            return new EndConversationResponse { Feedback = "Cuộc hội thoại đã kết thúc.", Score = 50 };
        }
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

    private class ConversationAiResponse
    {
        public string Text { get; set; } = "";
        public List<string> Suggestions { get; set; } = new();
        public List<VocabularyResponse> NewVocabulary { get; set; } = new();
        public List<string> GrammarPoints { get; set; } = new();
    }

    private class VocabularyResponse
    {
        public string Word { get; set; } = "";
        public string Reading { get; set; } = "";
        public string Meaning { get; set; } = "";
        public string Example { get; set; } = "";
        public string JlptLevel { get; set; } = "N5";
    }

    private class EndConversationResponse
    {
        public string Feedback { get; set; } = "";
        public int Score { get; set; } = 50;
    }
}
