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

    public static ConversationResultResponse ToResultResponse(
        this ConversationSession session,
        List<ConversationMessage> messages,
        string feedback,
        string duration)
    {
        var allVocabulary = messages
            .SelectMany(m => m.NewVocabulary)
            .Select(v => new VocabularyItemDto
            {
                Word = v.Word,
                Reading = v.Reading,
                Meaning = v.Meaning,
                Example = v.Example
            })
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
            Feedback = feedback,
            Score = session.Score
        };
    }
}
