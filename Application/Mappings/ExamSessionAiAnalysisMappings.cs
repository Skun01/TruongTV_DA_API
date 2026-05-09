using System.Text.Json;
using Application.DTOs.ExamSessions;
using Application.Helper;
using Domain.Constants;
using Domain.Entities;

namespace Application.Mappings;

public static class ExamSessionAiAnalysisMappings
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    public static JlptAiAnalysisResponse ToAiAnalysisResponse(this ExamSessionAiAnalysis analysis)
    {
        try
        {
            var response = JsonSerializer.Deserialize<JlptAiAnalysisResponse>(analysis.OutputJson, JsonOptions);
            return response ?? throw new ApplicationException(MessageConstants.ExamSessionMessage.AI_ANALYSIS_INVALID);
        }
        catch (JsonException)
        {
            throw new ApplicationException(MessageConstants.ExamSessionMessage.AI_ANALYSIS_INVALID);
        }
    }

    public static JlptAiAnalysisResponse ToAiAnalysisResponse(
        this JlptAiAnalysisContent content,
        ExamSession session,
        string analysisId,
        string model,
        string promptVersion,
        DateTime generatedAt)
    {
        var maxScore = session.Exam.Sections.Sum(x => x.MaxScore);
        var scorePercent = JlptAiAnalysisInputHelper.CalculatePercent(session.TotalScore ?? 0, maxScore);
        var questionDetails = BuildQuestionDetailMap(session);
        var wrongQuestionIds = questionDetails.Values
            .Where(x => !x.IsCorrect)
            .Select(x => x.QuestionId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new JlptAiAnalysisResponse
        {
            AnalysisId = analysisId,
            SessionId = session.Id,
            ExamId = session.ExamId,
            ExamTitle = session.Exam.Title,
            Level = session.Exam.Level.ToString(),
            Status = "Completed",
            GeneratedAt = generatedAt,
            Model = model,
            PromptVersion = promptVersion,
            Summary = new JlptAiAnalysisSummary
            {
                Headline = content.Summary.Headline,
                OverallBand = content.Summary.OverallBand,
                ScorePercent = scorePercent,
                Passed = session.IsPassed ?? false,
                EstimatedLevelReadiness = content.Summary.EstimatedLevelReadiness,
            },
            SectionAnalyses = BuildSectionAnalyses(content, session),
            MistakePatterns = content.MistakePatterns,
            QuestionInsights = BuildQuestionInsights(content, questionDetails),
            Recommendations = BuildRecommendations(content, session, wrongQuestionIds),
            NextActions = BuildNextActions(session, wrongQuestionIds.Count > 0),
        };
    }

    public static string ToOutputJson(this JlptAiAnalysisResponse response)
    {
        return JsonSerializer.Serialize(response, JsonOptions);
    }

    public static JlptAiAnalysisSummaryOnlyResponse ToSummaryOnlyResponse(this JlptAiAnalysisResponse response)
    {
        return new JlptAiAnalysisSummaryOnlyResponse
        {
            AnalysisId = response.AnalysisId,
            SessionId = response.SessionId,
            ExamId = response.ExamId,
            ExamTitle = response.ExamTitle,
            Level = response.Level,
            Status = response.Status,
            GeneratedAt = response.GeneratedAt,
            Summary = response.Summary,
            SectionAnalyses = response.SectionAnalyses,
            NextActions = response.NextActions,
        };
    }

    private static List<JlptAiSectionAnalysis> BuildSectionAnalyses(JlptAiAnalysisContent content, ExamSession session)
    {
        var sectionContentLookup = content.SectionAnalyses
            .GroupBy(x => x.SectionType, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
        var scoreLookup = session.SectionScores.ToDictionary(x => x.SectionId);

        return session.Exam.Sections
            .OrderBy(x => x.OrderIndex)
            .Select(section =>
            {
                sectionContentLookup.TryGetValue(section.SectionType.ToString(), out var item);
                scoreLookup.TryGetValue(section.Id, out var score);

                return new JlptAiSectionAnalysis
                {
                    SectionType = section.SectionType.ToString(),
                    Score = score?.Score ?? 0,
                    MaxScore = section.MaxScore,
                    PassScore = section.PassScore,
                    IsPassed = score?.IsPassed ?? false,
                    PerformanceBand = item?.PerformanceBand ?? "Weak",
                    Diagnosis = item?.Diagnosis ?? string.Empty,
                    Strengths = item?.Strengths ?? [],
                    Weaknesses = item?.Weaknesses ?? [],
                    RecommendedFocus = item?.RecommendedFocus ?? [],
                };
            })
            .ToList();
    }

    private static List<JlptAiQuestionInsight> BuildQuestionInsights(
        JlptAiAnalysisContent content,
        Dictionary<string, QuestionDetail> questionDetails)
    {
        var insights = content.QuestionInsights
            .Where(x => questionDetails.ContainsKey(x.QuestionId))
            .Select(x =>
            {
                var detail = questionDetails[x.QuestionId];
                return new JlptAiQuestionInsight
                {
                    QuestionId = detail.QuestionId,
                    SectionType = detail.SectionType,
                    IsCorrect = detail.IsCorrect,
                    SelectedOptionId = detail.SelectedOptionId,
                    CorrectOptionId = detail.CorrectOptionId,
                    RootCause = x.RootCause,
                    Explanation = x.Explanation,
                    ReviewTags = x.ReviewTags,
                };
            })
            .OrderBy(x => questionDetails[x.QuestionId].Order)
            .ToList();

        if (insights.Count > 0)
            return insights;

        return questionDetails.Values
            .Where(x => !x.IsCorrect)
            .OrderBy(x => x.Order)
            .Take(3)
            .Select(detail => new JlptAiQuestionInsight
            {
                QuestionId = detail.QuestionId,
                SectionType = detail.SectionType,
                IsCorrect = detail.IsCorrect,
                SelectedOptionId = detail.SelectedOptionId,
                CorrectOptionId = detail.CorrectOptionId,
                RootCause = "Bạn nên đối chiếu lại logic giữa đáp án đã chọn và explanation của câu này.",
                Explanation = string.IsNullOrWhiteSpace(detail.Explanation)
                    ? "Hãy xem lại đáp án đúng và phân tích điểm khác biệt với lựa chọn của bạn."
                    : detail.Explanation!,
                ReviewTags = [detail.SectionType.ToLowerInvariant()],
            })
            .ToList();
    }

    private static List<JlptAiRecommendation> BuildRecommendations(
        JlptAiAnalysisContent content,
        ExamSession session,
        HashSet<string> wrongQuestionIds)
    {
        return content.Recommendations
            .Select(item => new JlptAiRecommendation
            {
                Type = item.Type,
                Priority = item.Priority,
                Title = item.Title,
                Reason = item.Reason,
                EstimatedMinutes = item.EstimatedMinutes,
                TargetRoute = ResolveRecommendationRoute(item.Type, session),
                TargetIds = item.TargetIds
                    .Where(wrongQuestionIds.Contains)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
            })
            .ToList();
    }

    private static List<JlptAiNextAction> BuildNextActions(ExamSession session, bool hasWrongQuestions)
    {
        var actions = new List<JlptAiNextAction>();

        if (hasWrongQuestions)
        {
            actions.Add(new JlptAiNextAction
            {
                Label = "Xem lại câu sai",
                ActionType = "ReviewWrongQuestions",
                TargetRoute = $"/jlpt-exams/{session.Id}/result?filter=wrong",
            });
        }

        if (!(session.IsPassed ?? false))
        {
            actions.Add(new JlptAiNextAction
            {
                Label = "Làm lại đề tương tự",
                ActionType = "RetakeExam",
                TargetRoute = $"/jlpt-exams/{session.ExamId}",
            });
        }

        actions.Add(new JlptAiNextAction
        {
            Label = "Quay về danh sách đề",
            ActionType = "BackToExamList",
            TargetRoute = "/jlpt-exams",
        });

        return actions;
    }

    private static string? ResolveRecommendationRoute(string recommendationType, ExamSession session)
    {
        return recommendationType == "RetakeExam"
            ? $"/jlpt-exams/{session.ExamId}"
            : $"/jlpt-exams/{session.Id}/result";
    }

    private static Dictionary<string, QuestionDetail> BuildQuestionDetailMap(ExamSession session)
    {
        var results = new Dictionary<string, QuestionDetail>(StringComparer.OrdinalIgnoreCase);
        var answerLookup = session.Answers.ToDictionary(x => x.QuestionId, StringComparer.OrdinalIgnoreCase);
        var order = 0;

        foreach (var section in session.Exam.Sections.OrderBy(x => x.OrderIndex))
        {
            foreach (var group in section.QuestionGroups.OrderBy(x => x.OrderIndex))
            {
                foreach (var question in group.Questions.OrderBy(x => x.OrderIndex))
                {
                    var correctOption = question.Options.FirstOrDefault(x => x.IsCorrect);
                    answerLookup.TryGetValue(question.Id, out var answer);

                    results[question.Id] = new QuestionDetail
                    {
                        QuestionId = question.Id,
                        SectionType = section.SectionType.ToString(),
                        SelectedOptionId = answer?.SelectedOptionId,
                        CorrectOptionId = correctOption?.Id ?? string.Empty,
                        IsCorrect = answer?.SelectedOptionId != null && answer.SelectedOptionId == correctOption?.Id,
                        Explanation = question.Explanation,
                        Order = order++,
                    };
                }
            }
        }

        return results;
    }

    private sealed class QuestionDetail
    {
        public string QuestionId { get; set; } = string.Empty;
        public string SectionType { get; set; } = string.Empty;
        public string? SelectedOptionId { get; set; }
        public string CorrectOptionId { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string? Explanation { get; set; }
        public int Order { get; set; }
    }
}
