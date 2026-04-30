namespace Application.DTOs.Questions;

public class ReorderQuestionsRequest
{
    public List<ReorderItem> Items { get; set; } = new();
}

public class ReorderItem
{
    public string Id { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}
