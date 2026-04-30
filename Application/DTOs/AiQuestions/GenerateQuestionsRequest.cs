namespace Application.DTOs.AiQuestions;

public class GenerateQuestionsRequest
{
    public string Level { get; set; } = string.Empty;
    public string SectionType { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int Count { get; set; } = 5;
    public string? QuestionGroupId { get; set; }
}
