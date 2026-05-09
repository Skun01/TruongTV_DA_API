namespace Application.DTOs.Questions;

public class ReorderQuestionsRequest
{
    public List<ReorderItem> Items { get; set; } = new();
}
