using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.DTOs.ExamSessions;
using Domain.Entities;
using Domain.Enums;

namespace Application.Helper;

public static class JlptAiAnalysisInputHelper
{
    public const int MaxQuestionCount = 60;
    public const int MaxCorrectPerSection = 5;
    public const int MaxInputBytes = 80 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static JlptAiAnalysisInput BuildInput(ExamSession session)
    {
        var sectionScoreMap = session.SectionScores.ToDictionary(x => x.SectionId, x => x);
        var candidates = new List<QuestionCandidate>();
        var globalOrder = 0;

        foreach (var section in session.Exam.Sections.OrderBy(x => x.OrderIndex))
        {
            sectionScoreMap.TryGetValue(section.Id, out var sectionScore);
            var resolvedSectionScore = sectionScore ?? new SessionSectionScore
            {
                SectionId = section.Id,
                Score = 0,
                MaxScore = section.MaxScore,
                PassScore = section.PassScore,
                IsPassed = false,
            };

            foreach (var group in section.QuestionGroups.OrderBy(x => x.OrderIndex))
            {
                foreach (var question in group.Questions.OrderBy(x => x.OrderIndex))
                {
                    var answer = session.Answers.FirstOrDefault(x => x.QuestionId == question.Id);
                    var correctOption = question.Options.FirstOrDefault(x => x.IsCorrect);
                    var selectedOption = answer?.SelectedOptionId == null
                        ? null
                        : question.Options.FirstOrDefault(x => x.Id == answer.SelectedOptionId) ?? answer.SelectedOption;
                    var isCorrect = answer?.SelectedOptionId != null && answer.SelectedOptionId == correctOption?.Id;

                    candidates.Add(new QuestionCandidate
                    {
                        SectionId = section.Id,
                        SectionType = section.SectionType.ToString(),
                        SectionScore = resolvedSectionScore.Score,
                        SectionMaxScore = resolvedSectionScore.MaxScore,
                        SectionPassScore = resolvedSectionScore.PassScore,
                        SectionIsPassed = resolvedSectionScore.IsPassed,
                        QuestionId = question.Id,
                        QuestionText = Trim(question.QuestionText, 320) ?? string.Empty,
                        PassageText = Trim(group.PassageText, 900),
                        Instruction = Trim(group.Instruction, 180),
                        SelectedOptionText = FormatOption(selectedOption),
                        CorrectOptionText = FormatOption(correctOption) ?? string.Empty,
                        IsCorrect = isCorrect,
                        Explanation = Trim(question.Explanation, 280),
                        GlobalOrder = globalOrder++,
                    });
                }
            }
        }

        var selectedQuestions = SelectQuestions(candidates);
        var maxScore = session.Exam.Sections.Sum(x => x.MaxScore);
        var durationMinutes = CalculateDurationMinutes(session);

        var input = new JlptAiAnalysisInput
        {
            Exam = new JlptAiAnalysisExamInput
            {
                SessionId = session.Id,
                ExamId = session.ExamId,
                Title = session.Exam.Title,
                Level = session.Exam.Level.ToString(),
                TotalScore = session.TotalScore ?? 0,
                MaxScore = maxScore,
                IsPassed = session.IsPassed ?? false,
                DurationMinutes = durationMinutes,
            },
            Sections = session.Exam.Sections
                .OrderBy(x => x.OrderIndex)
                .Select(section =>
                {
                    sectionScoreMap.TryGetValue(section.Id, out var score);
                    return new JlptAiAnalysisSectionInput
                    {
                        SectionId = section.Id,
                        SectionType = section.SectionType.ToString(),
                        Score = score?.Score ?? 0,
                        MaxScore = section.MaxScore,
                        PassScore = section.PassScore,
                        IsPassed = score?.IsPassed ?? false,
                    };
                })
                .ToList(),
            Questions = selectedQuestions
                .Select(x => new JlptAiAnalysisQuestionInput
                {
                    QuestionId = x.QuestionId,
                    SectionType = x.SectionType,
                    QuestionText = x.QuestionText,
                    PassageText = x.PassageText,
                    Instruction = x.Instruction,
                    SelectedOptionText = x.SelectedOptionText,
                    CorrectOptionText = x.CorrectOptionText,
                    IsCorrect = x.IsCorrect,
                    Explanation = x.Explanation,
                })
                .ToList(),
        };

        return EnsureSizeLimit(input);
    }

    public static string Serialize(JlptAiAnalysisInput input)
    {
        return JsonSerializer.Serialize(input, JsonOptions);
    }

    public static string ComputeInputHash(string serializedInput)
    {
        var bytes = Encoding.UTF8.GetBytes(serializedInput);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public static double CalculatePercent(int score, int maxScore)
    {
        if (maxScore <= 0)
            return 0;

        return Math.Round(score * 100d / maxScore, 2);
    }

    private static List<QuestionCandidate> SelectQuestions(List<QuestionCandidate> candidates)
    {
        var prioritizedIncorrect = candidates
            .Where(x => !x.IsCorrect)
            .OrderBy(x => x.SectionIsPassed)
            .ThenBy(x => CalculatePercent(x.SectionScore, x.SectionMaxScore))
            .ThenBy(x => x.SelectedOptionText != null)
            .ThenBy(x => x.GlobalOrder)
            .ToList();

        var selected = prioritizedIncorrect
            .Take(MaxQuestionCount)
            .ToList();

        if (selected.Count >= MaxQuestionCount)
            return selected.OrderBy(x => x.GlobalOrder).ToList();

        var selectedIds = selected
            .Select(x => x.QuestionId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var remainingSlots = MaxQuestionCount - selected.Count;

        var correctCandidates = candidates
            .Where(x => x.IsCorrect && !selectedIds.Contains(x.QuestionId))
            .GroupBy(x => x.SectionId)
            .SelectMany(group => group
                .OrderBy(x => x.GlobalOrder)
                .Take(MaxCorrectPerSection))
            .OrderBy(x => x.SectionIsPassed)
            .ThenBy(x => CalculatePercent(x.SectionScore, x.SectionMaxScore))
            .ThenBy(x => x.GlobalOrder)
            .Take(remainingSlots)
            .ToList();

        selected.AddRange(correctCandidates);

        return selected
            .OrderBy(x => x.GlobalOrder)
            .ToList();
    }

    private static JlptAiAnalysisInput EnsureSizeLimit(JlptAiAnalysisInput input)
    {
        if (GetByteCount(input) <= MaxInputBytes)
            return input;

        for (var index = input.Questions.Count - 1; index >= 0 && GetByteCount(input) > MaxInputBytes; index--)
        {
            if (input.Questions[index].IsCorrect)
                input.Questions.RemoveAt(index);
        }

        if (GetByteCount(input) <= MaxInputBytes)
            return input;

        foreach (var question in input.Questions.Where(x => x.IsCorrect))
            question.PassageText = null;

        if (GetByteCount(input) <= MaxInputBytes)
            return input;

        foreach (var question in input.Questions)
        {
            question.PassageText = Trim(question.PassageText, 420);
            question.QuestionText = Trim(question.QuestionText, 220) ?? string.Empty;
            question.Instruction = Trim(question.Instruction, 120);
            question.SelectedOptionText = Trim(question.SelectedOptionText, 80);
            question.CorrectOptionText = Trim(question.CorrectOptionText, 80) ?? string.Empty;
            question.Explanation = Trim(question.Explanation, 180);
        }

        while (input.Questions.Count > 1 && GetByteCount(input) > MaxInputBytes)
            input.Questions.RemoveAt(input.Questions.Count - 1);

        return input;
    }

    private static int GetByteCount(JlptAiAnalysisInput input)
    {
        return Encoding.UTF8.GetByteCount(Serialize(input));
    }

    private static int CalculateDurationMinutes(ExamSession session)
    {
        var endAt = session.SubmittedAt ?? session.ExpiresAt;
        var duration = endAt - session.StartedAt;

        if (duration <= TimeSpan.Zero)
            return 0;

        return (int)Math.Ceiling(duration.TotalMinutes);
    }

    private static string? FormatOption(QuestionOption? option)
    {
        if (option == null)
            return null;

        var text = StringHelper.NormalizeOptional(option.Text);
        return string.IsNullOrWhiteSpace(text)
            ? option.Label.ToString()
            : $"{option.Label}. {text}";
    }

    private static string? Trim(string? value, int maxLength)
    {
        var normalized = StringHelper.NormalizeOptional(value);
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength].Trim();
    }

    private sealed class QuestionCandidate
    {
        public string SectionId { get; set; } = string.Empty;
        public string SectionType { get; set; } = string.Empty;
        public int SectionScore { get; set; }
        public int SectionMaxScore { get; set; }
        public int SectionPassScore { get; set; }
        public bool SectionIsPassed { get; set; }
        public string QuestionId { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string? PassageText { get; set; }
        public string? Instruction { get; set; }
        public string? SelectedOptionText { get; set; }
        public string CorrectOptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string? Explanation { get; set; }
        public int GlobalOrder { get; set; }
    }
}
