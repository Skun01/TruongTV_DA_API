namespace Application.DTOs.Dashboard.Learner;

public class ExamHistoryResponse
{
    public List<ExamHistoryItem> Items { get; set; } = new();
    public ExamHistoryStats Stats { get; set; } = new();
}
