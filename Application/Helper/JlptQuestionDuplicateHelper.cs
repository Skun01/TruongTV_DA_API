using Application.DTOs.AiQuestions;
using Domain.Entities;

namespace Application.Helper;

public static class JlptQuestionDuplicateHelper
{
    public static List<AiGeneratedQuestionDuplicateCandidate> FindDuplicates(
        AiGeneratedQuestionData generatedData,
        IEnumerable<Question> existingQuestions,
        int maxResults = 3)
    {
        var sourceText = BuildCompositeText(generatedData);
        if (string.IsNullOrWhiteSpace(sourceText))
            return new List<AiGeneratedQuestionDuplicateCandidate>();

        var candidates = new List<AiGeneratedQuestionDuplicateCandidate>();
        foreach (var question in existingQuestions)
        {
            var candidateText = BuildCompositeText(question);
            if (string.IsNullOrWhiteSpace(candidateText))
                continue;

            var similarity = ComputeDiceCoefficient(sourceText, candidateText);
            if (similarity < 0.45)
                continue;

            candidates.Add(new AiGeneratedQuestionDuplicateCandidate
            {
                SourceType = "QuestionBank",
                SourceId = question.Id,
                PreviewText = BuildPreviewText(question),
                SimilarityScore = Math.Round(similarity, 4),
            });
        }

        return candidates
            .OrderByDescending(candidate => candidate.SimilarityScore)
            .Take(maxResults)
            .ToList();
    }

    private static string BuildCompositeText(AiGeneratedQuestionData data)
    {
        var question = data.Questions.FirstOrDefault();
        if (question == null)
            return string.Empty;

        return Normalize($"""
            {data.Passage}
            {data.Script}
            {question.QuestionText}
            {string.Join(" ", question.Options.Select(option => option.Text))}
            """);
    }

    private static string BuildCompositeText(Question question)
    {
        return Normalize($"""
            {question.Group?.PassageText}
            {question.Group?.AudioScript}
            {question.QuestionText}
            {string.Join(" ", question.Options.Select(option => option.Text))}
            """);
    }

    private static string BuildPreviewText(Question question)
    {
        var preview = question.QuestionText.Trim();
        return preview.Length <= 160 ? preview : $"{preview[..157]}...";
    }

    private static string Normalize(string text)
    {
        return string.Concat(text
            .Where(character => !char.IsWhiteSpace(character) && !char.IsPunctuation(character) && !char.IsSymbol(character)))
            .Trim()
            .ToLowerInvariant();
    }

    private static double ComputeDiceCoefficient(string left, string right)
    {
        if (left.Length < 2 || right.Length < 2)
            return string.Equals(left, right, StringComparison.Ordinal) ? 1 : 0;

        var leftBigrams = BuildBigrams(left);
        var rightBigrams = BuildBigrams(right);
        var intersection = leftBigrams.Intersect(rightBigrams).Count();
        return (2d * intersection) / (leftBigrams.Count + rightBigrams.Count);
    }

    private static HashSet<string> BuildBigrams(string value)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < value.Length - 1; index++)
            result.Add(value.Substring(index, 2));

        return result;
    }
}
