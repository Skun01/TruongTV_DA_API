using Application.DTOs.AiQuestions;
using Domain.Entities;

namespace Application.Mappings;

public static class AiQuestionMappings
{
    public static AiGeneratedQuestionResponse ToResponse(this AiGeneratedQuestion aiQuestion)
    {
        return new AiGeneratedQuestionResponse
        {
            Id = aiQuestion.Id,
            Level = aiQuestion.Level.ToString(),
            SectionType = aiQuestion.SectionType.ToString(),
            Topic = aiQuestion.Topic,
            GeneratedData = aiQuestion.GeneratedData,
            Status = aiQuestion.Status.ToString(),
            ReviewedBy = aiQuestion.ReviewedBy,
            ReviewerName = aiQuestion.Reviewer?.Username,
            ReviewedAt = aiQuestion.ReviewedAt,
            QuestionId = aiQuestion.QuestionId,
            CreatedBy = aiQuestion.CreatedBy,
            CreatorName = aiQuestion.Creator.Username,
            CreatedAt = aiQuestion.CreatedAt,
            UpdatedAt = aiQuestion.UpdatedAt,
        };
    }
}
