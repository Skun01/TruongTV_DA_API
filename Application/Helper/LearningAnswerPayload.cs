namespace Application.Helper;

public sealed record LearningAnswerPayload(
    string Prompt,
    string? QuestionText,
    string? SecondaryText,
    string? Hint,
    List<string> AcceptedAnswers,
    string? SelectedSentenceId,
    string QuestionSource,
    string? CompletedQuestionText,
    string? FrontText,
    string? BackText);
