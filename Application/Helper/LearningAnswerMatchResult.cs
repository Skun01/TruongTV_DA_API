namespace Application.Helper;

public sealed record LearningAnswerMatchResult(
    bool IsCorrect,
    List<string> SubmittedAnswers,
    List<string> NormalizedSubmittedAnswers,
    string? CanonicalAnswer);
