namespace Application.Helper;

public sealed record LearningAnswerPayload(
    string Prompt,
    string? QuestionText,
    string? SecondaryText,
    string? Hint,
    List<string> AcceptedAnswers,
    string? SelectedSentenceId,
    string? FrontText,
    string? BackText);
