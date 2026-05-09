using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Helper;

public static class LearningAnswerMatcher
{
    public static LearningAnswerMatchResult Match(IEnumerable<string> submittedAnswers, List<string> acceptedAnswers)
    {
        var submitted = submittedAnswers
            .Select(answer => answer.Trim())
            .Where(answer => !string.IsNullOrWhiteSpace(answer))
            .ToList();

        var normalizedSubmitted = submitted
            .Select(NormalizeForMatching)
            .Where(answer => !string.IsNullOrWhiteSpace(answer))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var normalizedAccepted = acceptedAnswers
            .Select(NormalizeForMatching)
            .Where(answer => !string.IsNullOrWhiteSpace(answer))
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        return new LearningAnswerMatchResult(
            normalizedSubmitted.Any(normalizedAccepted.Contains),
            submitted,
            normalizedSubmitted,
            acceptedAnswers.FirstOrDefault());
    }

    public static string NormalizeForMatching(string value)
    {
        var normalized = value
            .Trim()
            .Normalize(NormalizationForm.FormKC);

        normalized = Regex.Replace(normalized, @"\s+", " ");
        return normalized.ToLower(CultureInfo.InvariantCulture);
    }
}
