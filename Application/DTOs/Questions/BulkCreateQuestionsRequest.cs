namespace Application.DTOs.Questions;

public class BulkCreateQuestionsRequest
{
    public List<CreateQuestionRequest> Questions { get; set; } = new();
}
