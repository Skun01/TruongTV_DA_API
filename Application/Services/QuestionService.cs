using Application.Common;
using Application.DTOs.Questions;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class QuestionService : IQuestionService
{
    private readonly IUnitOfWork _unitOfWork;

    public QuestionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<QuestionResponse> CreateQuestionAsync(CreateQuestionRequest request)
    {
        var group = await _unitOfWork.QuestionGroups.GetByIdAsync(request.GroupId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.GROUP_NOT_FOUND);

        var question = BuildQuestion(request);
        await _unitOfWork.Questions.AddAsync(question);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Questions.GetDetailByIdAsync(question.Id)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.QUESTION_NOT_FOUND);

        return created.ToQuestionResponse();
    }

    public async Task<(List<QuestionResponse> Items, MetaData Meta)> SearchQuestionsAsync(QuestionSearchQuery query)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);
        var level = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);
        var sectionType = EnumParsingHelper.ParseNullable<SectionType>(query.SectionType);

        var (items, total) = await _unitOfWork.Questions.SearchAsync(
            query.Keyword,
            level,
            sectionType,
            page,
            pageSize);

        return (
            items.Select(x => x.ToQuestionResponse()).ToList(),
            new MetaData
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
            });
    }

    public async Task<QuestionResponse> GetQuestionDetailAsync(string id)
    {
        var question = await _unitOfWork.Questions.GetDetailByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.QUESTION_NOT_FOUND);

        return question.ToQuestionResponse();
    }

    public async Task<QuestionResponse> UpdateQuestionAsync(string id, UpdateQuestionRequest request)
    {
        var question = await _unitOfWork.Questions.GetDetailByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.QUESTION_NOT_FOUND);

        // Refetch với tracking
        var questionEntity = await _unitOfWork.Questions.GetByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.QUESTION_NOT_FOUND);

        questionEntity.QuestionText = request.QuestionText.Trim();
        questionEntity.ImageUrl = request.ImageUrl;
        questionEntity.ImageCaption = request.ImageCaption;
        questionEntity.Explanation = request.Explanation;
        questionEntity.Score = request.Score;
        questionEntity.OrderIndex = request.OrderIndex;
        questionEntity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Questions.UpdateAsync(questionEntity);

        // Xóa options cũ và tạo mới
        foreach (var oldOption in question.Options)
        {
            var optionEntity = await _unitOfWork.QuestionOptions.GetByIdAsync(oldOption.Id);
            if (optionEntity != null)
                _unitOfWork.QuestionOptions.DeleteAsync(optionEntity);
        }

        foreach (var optionReq in request.Options)
        {
            var label = EnumParsingHelper.ParseRequired<OptionLabel>(optionReq.Label);
            var optionType = EnumParsingHelper.ParseRequired<OptionType>(optionReq.OptionType);

            var option = new QuestionOption
            {
                Id = optionReq.Id ?? Guid.NewGuid().ToString(),
                QuestionId = id,
                Label = label,
                Text = optionReq.Text,
                ImageUrl = optionReq.ImageUrl,
                OptionType = optionType,
                IsCorrect = optionReq.IsCorrect,
            };

            await _unitOfWork.QuestionOptions.AddAsync(option);
        }

        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Questions.GetDetailByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.QUESTION_NOT_FOUND);

        return updated.ToQuestionResponse();
    }

    public async Task DeleteQuestionAsync(string id)
    {
        var question = await _unitOfWork.Questions.GetByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.QUESTION_NOT_FOUND);

        _unitOfWork.Questions.DeleteAsync(question);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<QuestionResponse>> BulkCreateQuestionsAsync(string groupId, BulkCreateQuestionsRequest request)
    {
        var group = await _unitOfWork.QuestionGroups.GetByIdAsync(groupId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.GROUP_NOT_FOUND);

        var createdIds = new List<string>();

        foreach (var questionReq in request.Questions)
        {
            questionReq.GroupId = groupId;
            var question = BuildQuestion(questionReq);
            await _unitOfWork.Questions.AddAsync(question);
            createdIds.Add(question.Id);
        }

        await _unitOfWork.SaveChangesAsync();

        var results = new List<QuestionResponse>();
        foreach (var qId in createdIds)
        {
            var q = await _unitOfWork.Questions.GetDetailByIdAsync(qId);
            if (q != null)
                results.Add(q.ToQuestionResponse());
        }

        return results;
    }

    public async Task ReorderQuestionsAsync(string groupId, ReorderQuestionsRequest request)
    {
        var group = await _unitOfWork.QuestionGroups.GetWithQuestionsAsync(groupId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.GROUP_NOT_FOUND);

        var questionIds = group.Questions.Select(q => q.Id).ToHashSet();
        var reorderIds = request.Items.Select(i => i.Id).ToHashSet();

        // Kiểm tra tất cả ID trong request đều thuộc group
        if (!reorderIds.IsSubsetOf(questionIds))
            throw new AppException(MessageConstants.ExamMessage.INVALID_REORDER_PAYLOAD, 400);

        foreach (var item in request.Items)
        {
            var question = await _unitOfWork.Questions.GetByIdAsync(item.Id);
            if (question != null)
            {
                question.OrderIndex = item.OrderIndex;
                question.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Questions.UpdateAsync(question);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Tạo entity Question từ request DTO
    /// </summary>
    private static Question BuildQuestion(CreateQuestionRequest request)
    {
        var question = new Question
        {
            Id = Guid.NewGuid().ToString(),
            GroupId = request.GroupId,
            QuestionText = request.QuestionText.Trim(),
            ImageUrl = request.ImageUrl,
            ImageCaption = request.ImageCaption,
            Explanation = request.Explanation,
            Score = request.Score,
            OrderIndex = request.OrderIndex,
        };

        foreach (var optionReq in request.Options)
        {
            var label = EnumParsingHelper.ParseRequired<OptionLabel>(optionReq.Label);
            var optionType = EnumParsingHelper.ParseRequired<OptionType>(optionReq.OptionType);

            question.Options.Add(new QuestionOption
            {
                Id = Guid.NewGuid().ToString(),
                QuestionId = question.Id,
                Label = label,
                Text = optionReq.Text,
                ImageUrl = optionReq.ImageUrl,
                OptionType = optionType,
                IsCorrect = optionReq.IsCorrect,
            });
        }

        return question;
    }
}
