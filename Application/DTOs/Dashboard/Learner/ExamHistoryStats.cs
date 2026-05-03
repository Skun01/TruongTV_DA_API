namespace Application.DTOs.Dashboard.Learner;

public class ExamHistoryStats
{
    public int TotalExamsTaken { get; set; }
    public int TotalPassed { get; set; }
    public int TotalFailed { get; set; }
    public double AverageScore { get; set; }
    public double PassRate { get; set; }
}
