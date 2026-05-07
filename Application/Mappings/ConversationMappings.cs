using Application.DTOs.Conversations;
using Domain.Entities;

namespace Application.Mappings;

public static class ConversationMappings
{
    public static ScenarioListResponse GetScenarios()
    {
        return new ScenarioListResponse
        {
            Scenarios = new List<ScenarioItemDto>
            {
                new() { Id = "Shopping", Name = "Đi Shopping", Icon = "🛍️", Description = "Mua sắm ở cửa hàng" },
                new() { Id = "Interview", Name = "Phỏng vấn xin việc", Icon = "💼", Description = "Ứng tuyển công việc" },
                new() { Id = "Direction", Name = "Hỏi đường", Icon = "📍", Description = "Hỏi đường ở Nhật" },
                new() { Id = "Meeting", Name = "Gặp gỡ bạn mới", Icon = "🤝", Description = "Làm quen bạn mới" },
                new() { Id = "Restaurant", Name = "Nhà hàng", Icon = "🍜", Description = "Ăn uống ở nhà hàng" },
                new() { Id = "Custom", Name = "Tự nhập kịch bản", Icon = "✏️", Description = "Nhập kịch bản tùy chỉnh" }
            }
        };
    }

    public static ConversationListItemResponse ToListItemResponse(this ConversationSession session)
    {
        return new ConversationListItemResponse
        {
            ConversationId = session.Id,
            Scenario = session.Scenario,
            Level = session.Level,
            Status = session.Status,
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt,
            TotalMessages = session.TotalMessages,
            Score = session.Score
        };
    }

    public static ConversationDetailResponse ToDetailResponse(
        this ConversationSession session,
        List<ConversationMessage> messages,
        string scenarioText)
    {
        return new ConversationDetailResponse
        {
            ConversationId = session.Id,
            Scenario = session.Scenario,
            CustomScenario = session.CustomScenario,
            ScenarioText = scenarioText,
            Level = session.Level,
            Status = session.Status,
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt,
            TotalMessages = session.TotalMessages,
            UserMessagesCount = session.UserMessagesCount,
            Score = session.Score,
            Messages = messages.Select(x => x.ToMessageItemResponse()).ToList()
        };
    }

    public static StartConversationResponse ToStartConversationResponse(this ConversationMessage aiMessage, string conversationId)
    {
        return new StartConversationResponse
        {
            ConversationId = conversationId,
            AiMessage = aiMessage.ToAiMessageDto()
        };
    }

    public static SendMessageResponse ToSendMessageResponse(
        this ConversationMessage aiMessage,
        int totalMessages,
        int userMessagesCount,
        int newWordsLearned)
    {
        return new SendMessageResponse
        {
            AiMessage = aiMessage.ToSendAiMessageDto(),
            Summary = new ConversationSummaryDto
            {
                TotalMessages = totalMessages,
                UserMessagesCount = userMessagesCount,
                NewWordsLearned = newWordsLearned
            }
        };
    }

    public static ConversationResultResponse ToResultResponse(
        this ConversationSession session,
        List<ConversationMessage> messages,
        string duration)
    {
        var allVocabulary = messages
            .SelectMany(m => m.NewVocabulary)
            .Select(v => v.ToVocabularyItemDto())
            .DistinctBy(v => v.Word)
            .ToList();

        var allGrammarPoints = messages
            .SelectMany(m => m.GrammarPoints)
            .Distinct()
            .ToList();

        return new ConversationResultResponse
        {
            ConversationId = session.Id,
            Scenario = session.Scenario,
            Level = session.Level,
            Duration = duration,
            TotalMessages = session.TotalMessages,
            NewVocabulary = allVocabulary,
            GrammarPoints = allGrammarPoints,
            Feedback = session.Feedback ?? string.Empty,
            Score = session.Score
        };
    }

    public static AiMessageDto ToAiMessageDto(this ConversationMessage message)
    {
        return new AiMessageDto
        {
            Text = message.Text,
            Suggestions = message.Suggestions ?? new List<string>()
        };
    }

    public static SendAiMessageDto ToSendAiMessageDto(this ConversationMessage message)
    {
        return new SendAiMessageDto
        {
            Text = message.Text,
            Suggestions = message.Suggestions ?? new List<string>(),
            NewVocabulary = message.NewVocabulary.Select(x => x.ToVocabularyItemDto()).ToList(),
            GrammarPoints = message.GrammarPoints
        };
    }

    public static ConversationMessageItemResponse ToMessageItemResponse(this ConversationMessage message)
    {
        return new ConversationMessageItemResponse
        {
            MessageId = message.Id,
            Sender = message.Sender,
            Text = message.Text,
            Suggestions = message.Suggestions ?? new List<string>(),
            NewVocabulary = message.NewVocabulary.Select(x => x.ToVocabularyItemDto()).ToList(),
            GrammarPoints = message.GrammarPoints,
            CreatedAt = message.CreatedAt
        };
    }

    public static VocabularyItemDto ToVocabularyItemDto(this ExtractedVocabulary vocabulary)
    {
        return new VocabularyItemDto
        {
            Word = vocabulary.Word,
            Reading = vocabulary.Reading,
            Meaning = vocabulary.Meaning,
            Example = vocabulary.Example
        };
    }

    public static int CountLearnedWords(this IEnumerable<ConversationMessage> messages)
    {
        return messages
            .SelectMany(x => x.NewVocabulary)
            .Select(x => x.Word)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
    }
}
