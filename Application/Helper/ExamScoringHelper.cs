using Domain.Entities;

namespace Application.Helper;

/// <summary>
/// Tính điểm bài thi JLPT — mỗi section có điểm sàn riêng, trượt 1 section = trượt toàn bài
/// </summary>
public static class ExamScoringHelper
{
    /// <summary>
    /// Tính điểm toàn bộ bài thi và trả về danh sách điểm từng section
    /// </summary>
    public static (int TotalScore, bool IsPassed, List<SessionSectionScore> SectionScores) CalculateScore(
        ExamSession session,
        Exam exam)
    {
        var sectionScores = new List<SessionSectionScore>();
        var totalScore = 0;
        var allSectionsPassed = true;

        // Tạo dictionary đáp án theo questionId
        var answerMap = session.Answers
            .Where(a => a.SelectedOptionId != null)
            .ToDictionary(a => a.QuestionId, a => a.SelectedOptionId!);

        foreach (var section in exam.Sections)
        {
            var sectionScore = CalculateSectionScore(section, answerMap);

            var sectionResult = new SessionSectionScore
            {
                Id = Guid.NewGuid().ToString(),
                SessionId = session.Id,
                SectionId = section.Id,
                Score = sectionScore,
                MaxScore = section.MaxScore,
                PassScore = section.PassScore,
                IsPassed = sectionScore >= section.PassScore,
            };

            if (!sectionResult.IsPassed)
                allSectionsPassed = false;

            totalScore += sectionScore;
            sectionScores.Add(sectionResult);
        }

        // Tổng điểm pass = tất cả section đều pass
        var isPassed = allSectionsPassed;

        return (totalScore, isPassed, sectionScores);
    }

    /// <summary>
    /// Tính điểm cho một section dựa trên số câu đúng và phân bố điểm
    /// </summary>
    private static int CalculateSectionScore(ExamSection section, Dictionary<string, string> answerMap)
    {
        var allQuestions = section.QuestionGroups
            .SelectMany(g => g.Questions)
            .ToList();

        if (allQuestions.Count == 0)
            return 0;

        var score = 0;

        foreach (var question in allQuestions)
        {
            if (!answerMap.TryGetValue(question.Id, out var selectedOptionId))
                continue;

            var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
            if (correctOption != null && correctOption.Id == selectedOptionId)
            {
                score += question.Score;
            }
        }

        // Đảm bảo điểm không vượt MaxScore
        return Math.Min(score, section.MaxScore);
    }
}
