using System.Text.Json;
using Application.Common;
using Application.DTOs.AiQuestions;
using Domain.Constants;
using Domain.Enums;

namespace Application.Helper;

public static class JlptQuestionGenerationValidationHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    public static bool TryParseAndValidate(
        string json,
        JlptLevel level,
        SectionType sectionType,
        int expectedCount,
        out AiGeneratedQuestionData? data,
        out List<string> errors,
        out List<string> warnings)
    {
        errors = new List<string>();
        warnings = new List<string>();
        data = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            errors.Add("JSON output is empty.");
            return false;
        }

        try
        {
            data = JsonSerializer.Deserialize<AiGeneratedQuestionData>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            errors.Add($"JSON parse failed: {ex.Message}");
            return false;
        }

        if (data == null)
        {
            errors.Add("JSON payload is null after deserialize.");
            return false;
        }

        Normalize(data, level, sectionType);

        if (data.Questions.Count != expectedCount)
            errors.Add($"Expected {expectedCount} questions but got {data.Questions.Count}.");

        if (sectionType == SectionType.Dokkai && string.IsNullOrWhiteSpace(data.Passage))
            errors.Add("Dokkai requires a non-empty passage.");

        if (sectionType == SectionType.Choukai && string.IsNullOrWhiteSpace(data.Script))
            errors.Add("Choukai requires a non-empty script.");

        if (sectionType != SectionType.Dokkai && !string.IsNullOrWhiteSpace(data.Passage))
            warnings.Add("Passage is present for a non-Dokkai question.");

        if (sectionType != SectionType.Choukai && !string.IsNullOrWhiteSpace(data.Script))
            warnings.Add("Script is present for a non-Choukai question.");

        ValidateLengthHints(data, level, sectionType, warnings);

        for (var index = 0; index < data.Questions.Count; index++)
        {
            var question = data.Questions[index];
            var questionNo = index + 1;

            if (string.IsNullOrWhiteSpace(question.QuestionText))
                errors.Add($"Question {questionNo} is missing questionText.");

            if (string.IsNullOrWhiteSpace(question.Explanation))
                errors.Add($"Question {questionNo} is missing explanation.");

            if (question.Options.Count != 4)
                errors.Add($"Question {questionNo} must contain exactly 4 options.");

            var labels = new HashSet<string>(StringComparer.Ordinal);
            var correctCount = 0;

            foreach (var option in question.Options)
            {
                if (string.IsNullOrWhiteSpace(option.Label))
                {
                    errors.Add($"Question {questionNo} has an option without label.");
                    continue;
                }

                if (!new[] { "A", "B", "C", "D" }.Contains(option.Label))
                    errors.Add($"Question {questionNo} has invalid option label '{option.Label}'.");

                if (!labels.Add(option.Label))
                    errors.Add($"Question {questionNo} has duplicate option label '{option.Label}'.");

                if (string.IsNullOrWhiteSpace(option.Text))
                    errors.Add($"Question {questionNo} option {option.Label} is empty.");

                if (option.IsCorrect)
                    correctCount++;
            }

            if (correctCount != 1)
                errors.Add($"Question {questionNo} must have exactly 1 correct option, but got {correctCount}.");

            var distinctOptionTexts = question.Options
                .Select(option => NormalizeForComparison(option.Text))
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Distinct(StringComparer.Ordinal)
                .Count();

            if (distinctOptionTexts != question.Options.Count)
                warnings.Add($"Question {questionNo} contains duplicate or near-empty option text.");

            if (question.SkillTags.Count == 0)
                warnings.Add($"Question {questionNo} is missing skillTags. Default tags were assigned.");
        }

        return errors.Count == 0;
    }

    public static AiGeneratedQuestionData EnsureValidOrThrow(
        string json,
        JlptLevel level,
        SectionType sectionType,
        int expectedCount)
    {
        if (!TryParseAndValidate(json, level, sectionType, expectedCount, out var data, out var errors, out _))
            throw new AppException(MessageConstants.AiQuestionMessage.INVALID_GENERATED_DATA, 400, details: errors);

        return data!;
    }

    public static string Serialize(AiGeneratedQuestionData data)
    {
        return JsonSerializer.Serialize(data, JsonOptions);
    }

    public static void ApplyMetadata(
        AiGeneratedQuestionData data,
        JlptLevel level,
        SectionType sectionType,
        IEnumerable<string> warnings,
        IReadOnlyCollection<AiGeneratedQuestionDuplicateCandidate> duplicates)
    {
        var duplicateList = duplicates
            .OrderByDescending(item => item.SimilarityScore)
            .Take(3)
            .ToList();

        var difficultyScore = data.Questions.Count == 0
            ? EstimateDifficultyScore(level, sectionType, data)
            : (int)Math.Round(data.Questions
                .Select(question => question.DifficultyScore ?? EstimateDifficultyScore(level, sectionType, data, question))
                .Average());

        data.Difficulty ??= new AiGeneratedQuestionDifficultyInfo();
        data.Difficulty.Level = level.ToString();
        data.Difficulty.Score = difficultyScore;
        data.Difficulty.Reason ??= "Estimated from JLPT level, section type, and text length.";

        var warningList = warnings
            .Where(warning => !string.IsNullOrWhiteSpace(warning))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var qualityScore = 100
            - (warningList.Count * 6)
            - duplicateList.Count(candidate => candidate.SimilarityScore >= 0.85) * 18
            - duplicateList.Count(candidate => candidate.SimilarityScore >= 0.7 && candidate.SimilarityScore < 0.85) * 8;

        data.Metadata = new AiGeneratedQuestionMetadata
        {
            IsValid = true,
            QualityScore = Math.Clamp(qualityScore, 0, 100),
            RequiresManualReview = warningList.Count > 0 || duplicateList.Any(candidate => candidate.SimilarityScore >= 0.75),
            ValidationWarnings = warningList,
            DuplicateCandidates = duplicateList,
        };
    }

    private static void Normalize(AiGeneratedQuestionData data, JlptLevel level, SectionType sectionType)
    {
        data.Passage = string.IsNullOrWhiteSpace(data.Passage) ? null : data.Passage.Trim();
        data.Script = string.IsNullOrWhiteSpace(data.Script) ? null : data.Script.Trim();
        data.Difficulty ??= new AiGeneratedQuestionDifficultyInfo();
        data.Difficulty.Level = string.IsNullOrWhiteSpace(data.Difficulty.Level) ? level.ToString() : data.Difficulty.Level.Trim();

        foreach (var question in data.Questions)
        {
            question.QuestionText = question.QuestionText?.Trim() ?? string.Empty;
            question.Explanation = string.IsNullOrWhiteSpace(question.Explanation) ? null : question.Explanation.Trim();
            question.SkillTags = question.SkillTags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (question.SkillTags.Count == 0)
                question.SkillTags = BuildDefaultSkillTags(sectionType);

            foreach (var option in question.Options)
            {
                option.Label = option.Label?.Trim().ToUpperInvariant() ?? string.Empty;
                option.Text = option.Text?.Trim() ?? string.Empty;
            }

            question.Options = question.Options
                .OrderBy(option => option.Label, StringComparer.Ordinal)
                .ToList();

            question.DifficultyScore ??= EstimateDifficultyScore(level, sectionType, data, question);
        }
    }

    private static void ValidateLengthHints(
        AiGeneratedQuestionData data,
        JlptLevel level,
        SectionType sectionType,
        List<string> warnings)
    {
        if (sectionType == SectionType.Dokkai && !string.IsNullOrWhiteSpace(data.Passage))
        {
            var length = data.Passage.Length;
            var (min, max) = GetRecommendedPassageLength(level);
            if (length < min || length > max)
                warnings.Add($"Passage length {length} is outside recommended range {min}-{max} for {level}.");
        }

        if (sectionType == SectionType.Choukai && !string.IsNullOrWhiteSpace(data.Script))
        {
            var length = data.Script.Length;
            var (min, max) = GetRecommendedScriptLength(level);
            if (length < min || length > max)
                warnings.Add($"Script length {length} is outside recommended range {min}-{max} for {level}.");
        }
    }

    private static int EstimateDifficultyScore(
        JlptLevel level,
        SectionType sectionType,
        AiGeneratedQuestionData data,
        AiGeneratedQuestionItem? question = null)
    {
        var baseScore = level switch
        {
            JlptLevel.N5 => 20,
            JlptLevel.N4 => 35,
            JlptLevel.N3 => 50,
            JlptLevel.N2 => 70,
            JlptLevel.N1 => 85,
            _ => 50,
        };

        var sectionAdjustment = sectionType switch
        {
            SectionType.Moji => -4,
            SectionType.Bunpou => 0,
            SectionType.Dokkai => 6,
            SectionType.Choukai => 8,
            _ => 0,
        };

        var passageLength = !string.IsNullOrWhiteSpace(data.Passage) ? data.Passage.Length / 40 : 0;
        var scriptLength = !string.IsNullOrWhiteSpace(data.Script) ? data.Script.Length / 45 : 0;
        var questionLength = question != null ? question.QuestionText.Length / 25 : 0;
        var optionLength = question != null && question.Options.Count > 0
            ? (int)Math.Round(question.Options.Average(option => option.Text.Length) / 20d)
            : 0;

        return Math.Clamp(baseScore + sectionAdjustment + passageLength + scriptLength + questionLength + optionLength, 0, 100);
    }

    private static (int Min, int Max) GetRecommendedPassageLength(JlptLevel level)
    {
        return level switch
        {
            JlptLevel.N5 => (100, 150),
            JlptLevel.N4 => (150, 220),
            JlptLevel.N3 => (200, 320),
            JlptLevel.N2 => (300, 520),
            JlptLevel.N1 => (500, 850),
            _ => (150, 300),
        };
    }

    private static (int Min, int Max) GetRecommendedScriptLength(JlptLevel level)
    {
        return level switch
        {
            JlptLevel.N5 => (80, 180),
            JlptLevel.N4 => (120, 240),
            JlptLevel.N3 => (180, 320),
            JlptLevel.N2 => (250, 420),
            JlptLevel.N1 => (350, 600),
            _ => (120, 300),
        };
    }

    private static List<string> BuildDefaultSkillTags(SectionType sectionType)
    {
        return sectionType switch
        {
            SectionType.Moji => new List<string> { "vocabulary", "kanji", "context" },
            SectionType.Bunpou => new List<string> { "grammar", "sentence-completion", "nuance" },
            SectionType.Dokkai => new List<string> { "reading", "main-idea", "detail" },
            SectionType.Choukai => new List<string> { "listening", "detail", "inference" },
            _ => new List<string> { "jlpt" },
        };
    }

    private static string NormalizeForComparison(string text)
    {
        return string.Concat(text
            .Where(character => !char.IsWhiteSpace(character) && !char.IsPunctuation(character) && !char.IsSymbol(character)))
            .Trim()
            .ToLowerInvariant();
    }
}
