using Application.DTOs.Exams;
using Application.DTOs.Questions;
using Domain.Entities;

namespace Application.Mappings;

public static class ExamMappings
{
    public static ExamListItemResponse ToListItemResponse(this Exam exam)
    {
        return new ExamListItemResponse
        {
            Id = exam.Id,
            Title = exam.Title,
            Level = exam.Level.ToString(),
            TotalDurationMinutes = exam.TotalDurationMinutes,
            Status = exam.Status.ToString(),
            SectionsCount = exam.Sections.Count,
            CreatedBy = exam.CreatedBy,
            CreatorName = exam.Creator.Username,
            CreatedAt = exam.CreatedAt,
            UpdatedAt = exam.UpdatedAt,
        };
    }

    public static ExamDetailResponse ToDetailResponse(this Exam exam)
    {
        return new ExamDetailResponse
        {
            Id = exam.Id,
            Title = exam.Title,
            Level = exam.Level.ToString(),
            TotalDurationMinutes = exam.TotalDurationMinutes,
            Status = exam.Status.ToString(),
            CreatedBy = exam.CreatedBy,
            CreatorName = exam.Creator.Username,
            Sections = exam.Sections
                .OrderBy(s => s.OrderIndex)
                .Select(s => s.ToSectionResponse())
                .ToList(),
            CreatedAt = exam.CreatedAt,
            UpdatedAt = exam.UpdatedAt,
        };
    }

    public static ExamSectionResponse ToSectionResponse(this ExamSection section)
    {
        return new ExamSectionResponse
        {
            Id = section.Id,
            SectionType = section.SectionType.ToString(),
            OrderIndex = section.OrderIndex,
            DurationMinutes = section.DurationMinutes,
            MaxScore = section.MaxScore,
            PassScore = section.PassScore,
            QuestionGroupsCount = section.QuestionGroups.Count,
            QuestionsCount = section.QuestionGroups.Sum(g => g.Questions.Count),
            QuestionGroups = section.QuestionGroups
                .OrderBy(g => g.OrderIndex)
                .Select(g => g.ToGroupResponse())
                .ToList(),
            CreatedAt = section.CreatedAt,
            UpdatedAt = section.UpdatedAt,
        };
    }

    public static QuestionGroupResponse ToGroupResponse(this QuestionGroup group)
    {
        return new QuestionGroupResponse
        {
            Id = group.Id,
            PassageText = group.PassageText,
            AudioUrl = group.AudioUrl,
            AudioScript = group.AudioScript,
            Instruction = group.Instruction,
            OrderIndex = group.OrderIndex,
            MondaiType = group.MondaiType?.ToString(),
            Questions = group.Questions
                .OrderBy(q => q.OrderIndex)
                .Select(q => q.ToQuestionResponse())
                .ToList(),
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt,
        };
    }
}
