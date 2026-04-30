using Application.Common;
using Application.DTOs.Exams;

namespace Application.IServices;

public interface IExamService
{
    // Exam CRUD
    Task<ExamDetailResponse> CreateExamAsync(CreateExamRequest request, string userId);
    Task<(List<ExamListItemResponse> Items, MetaData Meta)> SearchExamsAsync(ExamSearchQuery query);
    Task<ExamDetailResponse> GetExamDetailAsync(string id);
    Task<ExamDetailResponse> UpdateExamAsync(string id, UpdateExamRequest request);
    Task PublishExamAsync(string id);
    Task DeleteExamAsync(string id);

    // Section
    Task<ExamSectionResponse> CreateSectionAsync(string examId, CreateSectionRequest request);
    Task<ExamSectionResponse> UpdateSectionAsync(string examId, string sectionId, UpdateSectionRequest request);
    Task DeleteSectionAsync(string examId, string sectionId);

    // QuestionGroup
    Task<QuestionGroupResponse> CreateQuestionGroupAsync(string sectionId, CreateQuestionGroupRequest request);
    Task<QuestionGroupResponse> UpdateQuestionGroupAsync(string sectionId, string groupId, UpdateQuestionGroupRequest request);
    Task DeleteQuestionGroupAsync(string sectionId, string groupId);

    // TTS — sinh audio từ AudioScript cho Choukai group
    Task<QuestionGroupResponse> GenerateGroupAudioAsync(string groupId, string userId);
}
