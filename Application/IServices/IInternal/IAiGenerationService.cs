using Domain.Enums;

namespace Application.IServices.IInternal;

public interface IAiGenerationService
{
    /// <summary>
    /// Sinh câu hỏi JLPT bằng AI, trả về raw JSON string
    /// </summary>
    Task<string> GenerateQuestionsJsonAsync(
        JlptLevel level,
        SectionType sectionType,
        string topic,
        int count,
        CancellationToken cancellationToken = default);
}
