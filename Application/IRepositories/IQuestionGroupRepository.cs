using Domain.Entities;

namespace Application.IRepositories;

public interface IQuestionGroupRepository : IRepository<QuestionGroup>
{
    Task<QuestionGroup?> GetWithQuestionsAsync(string id);
}
