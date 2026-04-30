using Application.Common;
using Application.DTOs.AiQuestions;

namespace Application.IServices;

public interface IAiQuestionService
{
    Task<List<AiGeneratedQuestionResponse>> GenerateQuestionsAsync(GenerateQuestionsRequest request, string userId);
    Task<(List<AiGeneratedQuestionResponse> Items, MetaData Meta)> SearchAsync(AiQuestionSearchQuery query);
    Task<AiGeneratedQuestionResponse> GetDetailAsync(string id);
    Task<AiGeneratedQuestionResponse> ApproveAsync(string id, string userId);
    Task<AiGeneratedQuestionResponse> RejectAsync(string id, string userId);
    Task<AiGeneratedQuestionResponse> EditAsync(string id, EditAiQuestionRequest request, string userId);
}
