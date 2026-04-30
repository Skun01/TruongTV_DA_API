using Application.DTOs.ExamSessions;
using Domain.Entities;

namespace Application.Mappings;

public static class ExamSessionMappings
{
    /// <summary>
    /// Map exam thành response cho học viên khi bắt đầu làm bài — ẩn đáp án đúng
    /// </summary>
    public static SessionStartResponse ToStartResponse(this ExamSession session, Dictionary<string, string?>? savedAnswers = null)
    {
        return new SessionStartResponse
        {
            SessionId = session.Id,
            ExamId = session.ExamId,
            ExamTitle = session.Exam.Title,
            Level = session.Exam.Level.ToString(),
            StartedAt = session.StartedAt,
            ExpiresAt = session.ExpiresAt,
            Sections = session.Exam.Sections
                .OrderBy(s => s.OrderIndex)
                .Select(s => s.ToSessionSectionResponse(savedAnswers))
                .ToList(),
        };
    }

    public static SessionSectionResponse ToSessionSectionResponse(this ExamSection section, Dictionary<string, string?>? savedAnswers = null)
    {
        return new SessionSectionResponse
        {
            SectionId = section.Id,
            SectionType = section.SectionType.ToString(),
            OrderIndex = section.OrderIndex,
            DurationMinutes = section.DurationMinutes,
            QuestionGroups = section.QuestionGroups
                .OrderBy(g => g.OrderIndex)
                .Select(g => g.ToSessionGroupResponse(savedAnswers))
                .ToList(),
        };
    }

    public static SessionQuestionGroupResponse ToSessionGroupResponse(this QuestionGroup group, Dictionary<string, string?>? savedAnswers = null)
    {
        return new SessionQuestionGroupResponse
        {
            GroupId = group.Id,
            PassageText = group.PassageText,
            AudioUrl = group.AudioUrl,
            Instruction = group.Instruction,
            OrderIndex = group.OrderIndex,
            MondaiType = group.MondaiType?.ToString(),
            Questions = group.Questions
                .OrderBy(q => q.OrderIndex)
                .Select(q => q.ToSessionQuestionResponse(savedAnswers))
                .ToList(),
        };
    }

    public static SessionQuestionResponse ToSessionQuestionResponse(this Question question, Dictionary<string, string?>? savedAnswers = null)
    {
        string? selectedOptionId = null;
        savedAnswers?.TryGetValue(question.Id, out selectedOptionId);

        return new SessionQuestionResponse
        {
            QuestionId = question.Id,
            QuestionText = question.QuestionText,
            ImageUrl = question.ImageUrl,
            ImageCaption = question.ImageCaption,
            OrderIndex = question.OrderIndex,
            SelectedOptionId = selectedOptionId,
            Options = question.Options
                .OrderBy(o => o.Label)
                .Select(o => o.ToSessionOptionResponse())
                .ToList(),
        };
    }

    public static SessionOptionResponse ToSessionOptionResponse(this QuestionOption option)
    {
        return new SessionOptionResponse
        {
            OptionId = option.Id,
            Label = option.Label.ToString(),
            Text = option.Text,
            ImageUrl = option.ImageUrl,
            OptionType = option.OptionType.ToString(),
        };
    }

    public static SubmitSessionResponse ToSubmitResponse(this ExamSession session)
    {
        var allQuestions = session.Exam.Sections
            .SelectMany(s => s.QuestionGroups)
            .SelectMany(g => g.Questions)
            .ToList();

        var answeredQuestionIds = session.Answers
            .Where(a => a.SelectedOptionId != null)
            .Select(a => a.QuestionId)
            .ToHashSet();

        // Đếm câu đúng dựa vào answers đã lưu
        var correctCount = session.Answers
            .Count(a => a.SelectedOption != null && a.SelectedOption.IsCorrect);

        return new SubmitSessionResponse
        {
            SessionId = session.Id,
            TotalScore = session.TotalScore ?? 0,
            CorrectCount = correctCount,
            WrongCount = answeredQuestionIds.Count - correctCount,
            UnansweredCount = allQuestions.Count - answeredQuestionIds.Count,
            IsPassed = session.IsPassed ?? false,
            SectionScores = session.SectionScores
                .Select(ss => ss.ToSectionScoreResponse())
                .ToList(),
        };
    }

    public static SectionScoreResponse ToSectionScoreResponse(this SessionSectionScore score)
    {
        return new SectionScoreResponse
        {
            SectionId = score.SectionId,
            SectionType = score.Section.SectionType.ToString(),
            Score = score.Score,
            MaxScore = score.MaxScore,
            PassScore = score.PassScore,
            IsPassed = score.IsPassed,
        };
    }

    public static SessionResultResponse ToResultResponse(this ExamSession session)
    {
        var answerMap = session.Answers.ToDictionary(a => a.QuestionId, a => a.SelectedOptionId);

        var resultQuestions = session.Exam.Sections
            .OrderBy(s => s.OrderIndex)
            .SelectMany(s => s.QuestionGroups
                .OrderBy(g => g.OrderIndex)
                .SelectMany(g => g.Questions
                    .OrderBy(q => q.OrderIndex)
                    .Select(q =>
                    {
                        var correctOption = q.Options.FirstOrDefault(o => o.IsCorrect);
                        answerMap.TryGetValue(q.Id, out var selectedId);

                        return new ResultQuestionResponse
                        {
                            QuestionId = q.Id,
                            QuestionText = q.QuestionText,
                            ImageUrl = q.ImageUrl,
                            Explanation = q.Explanation,
                            SectionType = s.SectionType.ToString(),
                            SelectedOptionId = selectedId,
                            CorrectOptionId = correctOption?.Id,
                            IsCorrect = selectedId != null && selectedId == correctOption?.Id,
                            Options = q.Options
                                .OrderBy(o => o.Label)
                                .Select(o => o.ToSessionOptionResponse())
                                .ToList(),
                        };
                    })))
            .ToList();

        return new SessionResultResponse
        {
            SessionId = session.Id,
            ExamId = session.ExamId,
            ExamTitle = session.Exam.Title,
            Level = session.Exam.Level.ToString(),
            TotalScore = session.TotalScore ?? 0,
            IsPassed = session.IsPassed ?? false,
            StartedAt = session.StartedAt,
            SubmittedAt = session.SubmittedAt,
            SectionScores = session.SectionScores
                .Select(ss => ss.ToSectionScoreResponse())
                .ToList(),
            Questions = resultQuestions,
        };
    }

    public static SessionListItemResponse ToListItemResponse(this ExamSession session)
    {
        return new SessionListItemResponse
        {
            SessionId = session.Id,
            ExamId = session.ExamId,
            ExamTitle = session.Exam.Title,
            Level = session.Exam.Level.ToString(),
            Status = session.Status.ToString(),
            TotalScore = session.TotalScore,
            IsPassed = session.IsPassed,
            StartedAt = session.StartedAt,
            SubmittedAt = session.SubmittedAt,
        };
    }
}
