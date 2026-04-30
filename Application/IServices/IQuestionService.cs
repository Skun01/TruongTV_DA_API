using Application.Common;
using Application.DTOs.Questions;

namespace Application.IServices;

public interface IQuestionService
{
    Task<QuestionResponse> CreateQuestionAsync(CreateQuestionRequest request);
    Task<(List<QuestionResponse> Items, MetaData Meta)> SearchQuestionsAsync(QuestionSearchQuery query);
    Task<QuestionResponse> GetQuestionDetailAsync(string id);
    Task<QuestionResponse> UpdateQuestionAsync(string id, UpdateQuestionRequest request);
    Task DeleteQuestionAsync(string id);

    Task<List<QuestionResponse>> BulkCreateQuestionsAsync(string groupId, BulkCreateQuestionsRequest request);
    Task ReorderQuestionsAsync(string groupId, ReorderQuestionsRequest request);
}
