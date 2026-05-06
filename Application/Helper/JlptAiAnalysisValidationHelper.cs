using System.Text.Json;
using Application.DTOs.ExamSessions;
using Domain.Constants;

namespace Application.Helper;

public static class JlptAiAnalysisValidationHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly HashSet<string> OverallBands = new(StringComparer.Ordinal)
    {
        "Excellent", "Good", "NeedsPractice", "Weak"
    };

    private static readonly HashSet<string> EstimatedReadinessBands = new(StringComparer.Ordinal)
    {
        "Ready", "Borderline", "NotReady"
    };

    private static readonly HashSet<string> PerformanceBands = new(StringComparer.Ordinal)
    {
        "Strong", "Stable", "Weak", "Critical"
    };

    private static readonly HashSet<string> SeverityBands = new(StringComparer.Ordinal)
    {
        "Low", "Medium", "High"
    };

    private static readonly HashSet<string> RecommendationTypes = new(StringComparer.Ordinal)
    {
        "ReviewWrongQuestions",
        "ReviewSection",
        "StudyVocabulary",
        "StudyGrammar",
        "PracticeReading",
        "PracticeListening",
        "RetakeExam",
    };

    private static readonly HashSet<string> Priorities = new(StringComparer.Ordinal)
    {
        "Low", "Medium", "High"
    };

    private static readonly HashSet<string> NextActionTypes = new(StringComparer.Ordinal)
    {
        "ReviewWrongQuestions",
        "StartRemedialSession",
        "RetakeExam",
        "BackToExamList",
    };

    public static JlptAiAnalysisContent ParseAndNormalize(string json, JlptAiAnalysisInput input)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<JlptAiAnalysisContent>(json, JsonOptions);
            if (parsed == null)
                throw new ApplicationException(MessageConstants.ExamSessionMessage.AI_ANALYSIS_INVALID);

            return Normalize(parsed, input);
        }
        catch (JsonException)
        {
            throw new ApplicationException(MessageConstants.ExamSessionMessage.AI_ANALYSIS_INVALID);
        }
        catch (NotSupportedException)
        {
            throw new ApplicationException(MessageConstants.ExamSessionMessage.AI_ANALYSIS_INVALID);
        }
    }

    public static string Serialize(JlptAiAnalysisResponse response)
    {
        return JsonSerializer.Serialize(response, JsonOptions);
    }

    private static JlptAiAnalysisContent Normalize(JlptAiAnalysisContent content, JlptAiAnalysisInput input)
    {
        var scorePercent = JlptAiAnalysisInputHelper.CalculatePercent(input.Exam.TotalScore, input.Exam.MaxScore);
        var sectionTypes = input.Sections
            .Select(x => x.SectionType)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var questionSectionMap = input.Questions.ToDictionary(x => x.QuestionId, x => x.SectionType, StringComparer.OrdinalIgnoreCase);
        var wrongQuestionIds = input.Questions
            .Where(x => !x.IsCorrect)
            .Select(x => x.QuestionId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var normalized = new JlptAiAnalysisContent
        {
            Summary = NormalizeSummary(content.Summary, input, scorePercent),
            SectionAnalyses = NormalizeSectionAnalyses(content.SectionAnalyses, input),
            MistakePatterns = NormalizeMistakePatterns(content.MistakePatterns, sectionTypes, questionSectionMap, wrongQuestionIds),
            QuestionInsights = NormalizeQuestionInsights(content.QuestionInsights, input, wrongQuestionIds),
            Recommendations = NormalizeRecommendations(content.Recommendations, input, wrongQuestionIds),
            NextActions = NormalizeNextActions(content.NextActions),
        };

        if (string.IsNullOrWhiteSpace(normalized.Summary.Headline)
            && normalized.MistakePatterns.Count == 0
            && normalized.QuestionInsights.Count == 0
            && normalized.Recommendations.Count == 0)
        {
            throw new ApplicationException(MessageConstants.ExamSessionMessage.AI_ANALYSIS_INVALID);
        }

        return normalized;
    }

    private static JlptAiAnalysisSummary NormalizeSummary(JlptAiAnalysisSummary? summary, JlptAiAnalysisInput input, double scorePercent)
    {
        return new JlptAiAnalysisSummary
        {
            Headline = TrimOrFallback(summary?.Headline, BuildFallbackHeadline(input), 180),
            OverallBand = CoalesceAllowed(summary?.OverallBand, OverallBands, GetDefaultOverallBand(scorePercent)),
            ScorePercent = scorePercent,
            Passed = input.Exam.IsPassed,
            EstimatedLevelReadiness = CoalesceAllowed(summary?.EstimatedLevelReadiness, EstimatedReadinessBands, GetDefaultReadiness(scorePercent, input.Exam.IsPassed)),
        };
    }

    private static List<JlptAiSectionAnalysis> NormalizeSectionAnalyses(List<JlptAiSectionAnalysis>? items, JlptAiAnalysisInput input)
    {
        var itemLookup = (items ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.SectionType))
            .GroupBy(x => x.SectionType, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        return input.Sections
            .Select(section =>
            {
                itemLookup.TryGetValue(section.SectionType, out var item);
                var sectionPercent = JlptAiAnalysisInputHelper.CalculatePercent(section.Score, section.MaxScore);
                var defaultBand = GetDefaultSectionBand(sectionPercent, section.IsPassed);

                return new JlptAiSectionAnalysis
                {
                    SectionType = section.SectionType,
                    Score = section.Score,
                    MaxScore = section.MaxScore,
                    PassScore = section.PassScore,
                    IsPassed = section.IsPassed,
                    PerformanceBand = CoalesceAllowed(item?.PerformanceBand, PerformanceBands, defaultBand),
                    Diagnosis = TrimOrFallback(item?.Diagnosis, BuildFallbackDiagnosis(section.SectionType, defaultBand), 500),
                    Strengths = NormalizeUiList(item?.Strengths, BuildFallbackStrengths(section.SectionType, section.IsPassed), 4, 160),
                    Weaknesses = NormalizeUiList(item?.Weaknesses, BuildFallbackWeaknesses(section.SectionType, section.IsPassed), 4, 160),
                    RecommendedFocus = NormalizeUiList(item?.RecommendedFocus, BuildFallbackFocus(section.SectionType), 4, 160),
                };
            })
            .ToList();
    }

    private static List<JlptAiMistakePattern> NormalizeMistakePatterns(
        List<JlptAiMistakePattern>? items,
        HashSet<string> sectionTypes,
        Dictionary<string, string> questionSectionMap,
        HashSet<string> wrongQuestionIds)
    {
        var results = new List<JlptAiMistakePattern>();

        foreach (var item in items ?? [])
        {
            var questionIds = StringHelper.NormalizeList(item.QuestionIds)
                .Where(wrongQuestionIds.Contains)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (questionIds.Count == 0)
                continue;

            var filteredSectionTypes = StringHelper.NormalizeList(item.SectionTypes)
                .Where(sectionTypes.Contains)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (filteredSectionTypes.Count == 0)
            {
                filteredSectionTypes = questionIds
                    .Select(questionId => questionSectionMap[questionId])
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            results.Add(new JlptAiMistakePattern
            {
                PatternId = TrimOrFallback(item.PatternId, $"pattern-{results.Count + 1}", 60),
                Title = TrimOrFallback(item.Title, "Nhóm lỗi cần ôn lại", 120),
                Severity = CoalesceAllowed(item.Severity, SeverityBands, GetDefaultSeverity(questionIds.Count)),
                SectionTypes = filteredSectionTypes,
                QuestionIds = questionIds,
                Evidence = TrimOrFallback(item.Evidence, "Nhóm câu này cùng cho thấy bạn đang lặp lại một kiểu sai.", 500),
                Advice = TrimOrFallback(item.Advice, "Hãy xem lại explanation của từng câu rồi đối chiếu điểm khác nhau giữa đáp án đúng và đáp án bạn chọn.", 500),
            });
        }

        return results
            .Take(5)
            .ToList();
    }

    private static List<JlptAiQuestionInsight> NormalizeQuestionInsights(
        List<JlptAiQuestionInsight>? items,
        JlptAiAnalysisInput input,
        HashSet<string> wrongQuestionIds)
    {
        var questionLookup = input.Questions.ToDictionary(x => x.QuestionId, StringComparer.OrdinalIgnoreCase);

        return (items ?? [])
            .Where(x => wrongQuestionIds.Contains(x.QuestionId))
            .GroupBy(x => x.QuestionId, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var item = group.First();
                var question = questionLookup[group.Key];

                return new JlptAiQuestionInsight
                {
                    QuestionId = question.QuestionId,
                    SectionType = question.SectionType,
                    IsCorrect = false,
                    SelectedOptionId = null,
                    CorrectOptionId = string.Empty,
                    RootCause = TrimOrFallback(item.RootCause, BuildFallbackRootCause(question.SectionType), 300),
                    Explanation = TrimOrFallback(item.Explanation, question.Explanation ?? "Hãy đối chiếu lại explanation sẵn có của đề để hiểu vì sao đáp án đúng hợp lý hơn.", 500),
                    ReviewTags = NormalizeUiList(item.ReviewTags, [question.SectionType.ToLowerInvariant()], 5, 40),
                };
            })
            .Take(12)
            .ToList();
    }

    private static List<JlptAiRecommendation> NormalizeRecommendations(
        List<JlptAiRecommendation>? items,
        JlptAiAnalysisInput input,
        HashSet<string> wrongQuestionIds)
    {
        var results = new List<JlptAiRecommendation>();

        foreach (var item in items ?? [])
        {
            if (!RecommendationTypes.Contains(item.Type))
                continue;

            results.Add(new JlptAiRecommendation
            {
                Type = item.Type,
                Priority = CoalesceAllowed(item.Priority, Priorities, GetDefaultPriority(item.Type)),
                Title = TrimOrFallback(item.Title, BuildFallbackRecommendationTitle(item.Type), 120),
                Reason = TrimOrFallback(item.Reason, "Đây là phần nên ưu tiên ôn tiếp theo dựa trên kết quả bài làm hiện tại.", 320),
                EstimatedMinutes = Clamp(item.EstimatedMinutes, 5, 120, 20),
                TargetRoute = null,
                TargetIds = StringHelper.NormalizeList(item.TargetIds)
                    .Where(wrongQuestionIds.Contains)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(10)
                    .ToList(),
            });
        }

        if (results.Count > 0)
            return results.Take(5).ToList();

        return BuildFallbackRecommendations(input, wrongQuestionIds);
    }

    private static List<JlptAiNextAction> NormalizeNextActions(List<JlptAiNextAction>? items)
    {
        return (items ?? [])
            .Where(x => NextActionTypes.Contains(x.ActionType))
            .Select(item => new JlptAiNextAction
            {
                Label = TrimOrFallback(item.Label, "Tiếp tục ôn tập", 80),
                ActionType = item.ActionType,
                TargetRoute = null,
            })
            .Take(4)
            .ToList();
    }

    private static List<JlptAiRecommendation> BuildFallbackRecommendations(JlptAiAnalysisInput input, HashSet<string> wrongQuestionIds)
    {
        var weakestSection = input.Sections
            .OrderBy(x => x.IsPassed)
            .ThenBy(x => JlptAiAnalysisInputHelper.CalculatePercent(x.Score, x.MaxScore))
            .FirstOrDefault();

        if (weakestSection == null)
            return [];

        return
        [
            new JlptAiRecommendation
            {
                Type = GetRecommendationTypeBySection(weakestSection.SectionType),
                Priority = weakestSection.IsPassed ? "Medium" : "High",
                Title = $"Ôn lại phần {TranslateSectionType(weakestSection.SectionType)}",
                Reason = "Đây là phần có tỷ lệ điểm thấp nhất trong bài làm hiện tại.",
                EstimatedMinutes = 20,
                TargetIds = input.Questions
                    .Where(x => !x.IsCorrect && x.SectionType.Equals(weakestSection.SectionType, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.QuestionId)
                    .Where(wrongQuestionIds.Contains)
                    .Take(10)
                    .ToList(),
            }
        ];
    }

    private static string BuildFallbackHeadline(JlptAiAnalysisInput input)
    {
        var weakestSection = input.Sections
            .OrderBy(x => x.IsPassed)
            .ThenBy(x => JlptAiAnalysisInputHelper.CalculatePercent(x.Score, x.MaxScore))
            .FirstOrDefault();

        if (weakestSection == null)
            return "Bạn đã hoàn thành bài thi và nên xem lại các câu sai để củng cố nền tảng.";

        var translatedSection = TranslateSectionType(weakestSection.SectionType);

        return input.Exam.IsPassed
            ? $"Bạn đã qua bài thi nhưng vẫn nên củng cố thêm phần {translatedSection} để điểm số ổn định hơn."
            : $"Bạn cần ưu tiên ôn lại phần {translatedSection} vì đây là khu vực làm mất nhiều điểm nhất.";
    }

    private static string BuildFallbackDiagnosis(string sectionType, string band)
    {
        return band switch
        {
            "Strong" => $"Phần {TranslateSectionType(sectionType)} đang là điểm mạnh tương đối rõ của bạn.",
            "Stable" => $"Phần {TranslateSectionType(sectionType)} tương đối ổn nhưng vẫn còn vài lỗi lặp lại.",
            "Weak" => $"Phần {TranslateSectionType(sectionType)} đang có lỗ hổng cần ôn lại theo dạng bài.",
            _ => $"Phần {TranslateSectionType(sectionType)} là ưu tiên ôn tập cao nhất ở thời điểm này."
        };
    }

    private static List<string> BuildFallbackStrengths(string sectionType, bool isPassed)
    {
        return
        [
            isPassed
                ? $"Bạn vẫn giữ được mức xử lý cơ bản ở phần {TranslateSectionType(sectionType)}."
                : $"Bạn vẫn làm đúng một số câu nền tảng ở phần {TranslateSectionType(sectionType)}."
        ];
    }

    private static List<string> BuildFallbackWeaknesses(string sectionType, bool isPassed)
    {
        return
        [
            isPassed
                ? $"Độ ổn định ở phần {TranslateSectionType(sectionType)} chưa cao."
                : $"Bạn chưa ổn định ở các câu trọng tâm của phần {TranslateSectionType(sectionType)}."
        ];
    }

    private static List<string> BuildFallbackFocus(string sectionType)
    {
        return sectionType switch
        {
            "Moji" =>
            [
                "Ôn lại nghĩa của từ trong ngữ cảnh.",
                "So sánh các từ dễ nhầm."
            ],
            "Bunpou" =>
            [
                "Nhìn dấu hiệu ngữ pháp trước và sau chỗ trống.",
                "Ôn lại sắc thái khác nhau giữa các mẫu gần nghĩa."
            ],
            "Dokkai" =>
            [
                "Đọc kỹ câu trước và sau keyword.",
                "Chú ý từ nối như しかし, つまり, そのため."
            ],
            _ =>
            [
                "Nghe bắt keyword và quan hệ giữa các ý.",
                "Tập dự đoán đáp án trước khi nhìn lựa chọn."
            ]
        };
    }

    private static string BuildFallbackRootCause(string sectionType)
    {
        return sectionType switch
        {
            "Moji" => "Bạn cần phân biệt kỹ nghĩa và sắc thái của các lựa chọn gần nhau.",
            "Bunpou" => "Bạn đang chọn đáp án theo cảm giác thay vì đối chiếu cấu trúc ngữ pháp.",
            "Dokkai" => "Bạn đang bám keyword nhưng chưa kiểm tra đủ quan hệ logic trong đoạn.",
            _ => "Bạn cần nghe và đối chiếu đầy đủ ý chính thay vì bám vào một tín hiệu đơn lẻ."
        };
    }

    private static string GetDefaultOverallBand(double scorePercent)
    {
        if (scorePercent >= 85)
            return "Excellent";

        if (scorePercent >= 70)
            return "Good";

        if (scorePercent >= 50)
            return "NeedsPractice";

        return "Weak";
    }

    private static string GetDefaultReadiness(double scorePercent, bool passed)
    {
        if (passed || scorePercent >= 80)
            return "Ready";

        if (scorePercent >= 65)
            return "Borderline";

        return "NotReady";
    }

    private static string GetDefaultSectionBand(double scorePercent, bool isPassed)
    {
        if (scorePercent >= 85)
            return "Strong";

        if (scorePercent >= 60 && isPassed)
            return "Stable";

        if (scorePercent >= 40)
            return "Weak";

        return "Critical";
    }

    private static string GetDefaultSeverity(int questionCount)
    {
        if (questionCount >= 3)
            return "High";

        if (questionCount == 2)
            return "Medium";

        return "Low";
    }

    private static string GetDefaultPriority(string recommendationType)
    {
        return recommendationType switch
        {
            "RetakeExam" => "Low",
            "ReviewWrongQuestions" => "High",
            _ => "Medium"
        };
    }

    private static string BuildFallbackRecommendationTitle(string recommendationType)
    {
        return recommendationType switch
        {
            "ReviewWrongQuestions" => "Xem lại các câu sai",
            "ReviewSection" => "Ôn lại phần điểm yếu",
            "StudyVocabulary" => "Ôn lại nhóm từ vựng dễ nhầm",
            "StudyGrammar" => "Ôn lại mẫu ngữ pháp liên quan",
            "PracticeReading" => "Luyện lại đọc hiểu",
            "PracticeListening" => "Luyện lại nghe hiểu",
            "RetakeExam" => "Làm lại một đề tương tự",
            _ => "Tiếp tục ôn tập"
        };
    }

    private static string GetRecommendationTypeBySection(string sectionType)
    {
        return sectionType switch
        {
            "Moji" => "StudyVocabulary",
            "Bunpou" => "StudyGrammar",
            "Dokkai" => "PracticeReading",
            _ => "PracticeListening"
        };
    }

    private static string TranslateSectionType(string sectionType)
    {
        return sectionType switch
        {
            "Moji" => "từ vựng và chữ",
            "Bunpou" => "ngữ pháp",
            "Dokkai" => "đọc hiểu",
            "Choukai" => "nghe hiểu",
            _ => sectionType
        };
    }

    private static string CoalesceAllowed(string? value, HashSet<string> allowedValues, string fallbackValue)
    {
        var normalized = StringHelper.NormalizeOptional(value);
        return normalized != null && allowedValues.Contains(normalized)
            ? normalized
            : fallbackValue;
    }

    private static List<string> NormalizeUiList(List<string>? values, List<string> fallbackValues, int maxItems, int maxLength)
    {
        var normalized = StringHelper.NormalizeList(values)
            .Select(item => item.Length <= maxLength ? item : item[..maxLength].Trim())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Take(maxItems)
            .ToList();

        return normalized.Count > 0 ? normalized : fallbackValues;
    }

    private static string TrimOrFallback(string? value, string fallbackValue, int maxLength)
    {
        var normalized = StringHelper.NormalizeOptional(value);
        if (string.IsNullOrWhiteSpace(normalized))
            return fallbackValue;

        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength].Trim();
    }

    private static int Clamp(int value, int min, int max, int fallbackValue)
    {
        if (value <= 0)
            return fallbackValue;

        if (value < min)
            return min;

        if (value > max)
            return max;

        return value;
    }
}
