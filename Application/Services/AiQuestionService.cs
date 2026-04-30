using System.Text.Json;
using Application.Common;
using Application.DTOs.AiQuestions;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.IServices.IInternal;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class AiQuestionService : IAiQuestionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiGenerationService _aiGenerationService;

    public AiQuestionService(IUnitOfWork unitOfWork, IAiGenerationService aiGenerationService)
    {
        _unitOfWork = unitOfWork;
        _aiGenerationService = aiGenerationService;
    }

    public async Task<List<AiGeneratedQuestionResponse>> GenerateQuestionsAsync(GenerateQuestionsRequest request, string userId)
    {
        var level = EnumParsingHelper.ParseRequired<JlptLevel>(request.Level);
        var sectionType = EnumParsingHelper.ParseRequired<SectionType>(request.SectionType);

        // Gọi Anthropic API sinh câu hỏi
        var generatedJson = await _aiGenerationService.GenerateQuestionsJsonAsync(
            level, sectionType, request.Topic.Trim(), request.Count);

        // Parse JSON để tách từng câu hỏi thành record riêng
        var parsedData = JsonSerializer.Deserialize<AiGeneratedQuestionData>(generatedJson);

        var generatedQuestions = new List<AiGeneratedQuestion>();

        if (parsedData?.Questions != null)
        {
            foreach (var questionItem in parsedData.Questions)
            {
                // Mỗi câu hỏi lưu riêng — kèm passage/script nếu có (Dokkai/Choukai)
                var singleQuestionData = new AiGeneratedQuestionData
                {
                    Passage = parsedData.Passage,
                    Script = parsedData.Script,
                    Questions = new List<AiGeneratedQuestionItem> { questionItem }
                };

                var aiQuestion = new AiGeneratedQuestion
                {
                    Id = Guid.NewGuid().ToString(),
                    Level = level,
                    SectionType = sectionType,
                    Topic = request.Topic.Trim(),
                    GeneratedData = JsonSerializer.Serialize(singleQuestionData),
                    Status = AiQuestionStatus.Pending,
                    CreatedBy = userId,
                };

                await _unitOfWork.AiGeneratedQuestions.AddAsync(aiQuestion);
                generatedQuestions.Add(aiQuestion);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        // Refetch để có đầy đủ navigation properties
        var results = new List<AiGeneratedQuestionResponse>();
        foreach (var q in generatedQuestions)
        {
            var detail = await _unitOfWork.AiGeneratedQuestions.GetDetailByIdAsync(q.Id);
            if (detail != null)
                results.Add(detail.ToResponse());
        }

        return results;
    }

    public async Task<(List<AiGeneratedQuestionResponse> Items, MetaData Meta)> SearchAsync(AiQuestionSearchQuery query)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);
        var level = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);
        var sectionType = EnumParsingHelper.ParseNullable<SectionType>(query.SectionType);
        var status = EnumParsingHelper.ParseNullable<AiQuestionStatus>(query.Status);

        var (items, total) = await _unitOfWork.AiGeneratedQuestions.SearchAsync(
            level,
            sectionType,
            status,
            page,
            pageSize);

        return (
            items.Select(x => x.ToResponse()).ToList(),
            new MetaData
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
            });
    }

    public async Task<AiGeneratedQuestionResponse> GetDetailAsync(string id)
    {
        var aiQuestion = await _unitOfWork.AiGeneratedQuestions.GetDetailByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.AiQuestionMessage.NOT_FOUND);

        return aiQuestion.ToResponse();
    }

    public async Task<AiGeneratedQuestionResponse> ApproveAsync(string id, string userId)
    {
        var aiQuestion = await _unitOfWork.AiGeneratedQuestions.GetByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.AiQuestionMessage.NOT_FOUND);

        if (aiQuestion.Status != AiQuestionStatus.Pending && aiQuestion.Status != AiQuestionStatus.Edited)
            throw new AppException(MessageConstants.AiQuestionMessage.ALREADY_REVIEWED, 400);

        // Parse GeneratedData JSON → tạo Question entity thực tế
        var data = JsonSerializer.Deserialize<AiGeneratedQuestionData>(aiQuestion.GeneratedData);
        var questionItem = data?.Questions?.FirstOrDefault();

        if (questionItem != null)
        {
            var question = new Question
            {
                Id = Guid.NewGuid().ToString(),
                GroupId = string.Empty,  // Sẽ gán khi thêm vào group cụ thể
                QuestionText = questionItem.QuestionText,
                Explanation = questionItem.Explanation,
                Score = 1,
                OrderIndex = 0,
            };

            foreach (var optionItem in questionItem.Options)
            {
                if (Enum.TryParse<OptionLabel>(optionItem.Label, true, out var label))
                {
                    question.Options.Add(new QuestionOption
                    {
                        Id = Guid.NewGuid().ToString(),
                        QuestionId = question.Id,
                        Label = label,
                        Text = optionItem.Text,
                        OptionType = OptionType.Text,
                        IsCorrect = optionItem.IsCorrect,
                    });
                }
            }

            // Chỉ lưu Question nếu có group target
            // Nếu không có group, giữ QuestionId = null — giáo viên sẽ gán sau
            // Ở đây ta lưu question vào DB trước, giáo viên dùng endpoint riêng để gán vào group
            if (question.Options.Count > 0)
            {
                await _unitOfWork.Questions.AddAsync(question);
                aiQuestion.QuestionId = question.Id;
            }
        }

        aiQuestion.Status = AiQuestionStatus.Approved;
        aiQuestion.ReviewedBy = userId;
        aiQuestion.ReviewedAt = DateTime.UtcNow;
        aiQuestion.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.AiGeneratedQuestions.UpdateAsync(aiQuestion);
        await _unitOfWork.SaveChangesAsync();

        var result = await _unitOfWork.AiGeneratedQuestions.GetDetailByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.AiQuestionMessage.NOT_FOUND);

        return result.ToResponse();
    }

    public async Task<AiGeneratedQuestionResponse> RejectAsync(string id, string userId)
    {
        var aiQuestion = await _unitOfWork.AiGeneratedQuestions.GetByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.AiQuestionMessage.NOT_FOUND);

        if (aiQuestion.Status != AiQuestionStatus.Pending && aiQuestion.Status != AiQuestionStatus.Edited)
            throw new AppException(MessageConstants.AiQuestionMessage.ALREADY_REVIEWED, 400);

        aiQuestion.Status = AiQuestionStatus.Rejected;
        aiQuestion.ReviewedBy = userId;
        aiQuestion.ReviewedAt = DateTime.UtcNow;
        aiQuestion.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.AiGeneratedQuestions.UpdateAsync(aiQuestion);
        await _unitOfWork.SaveChangesAsync();

        var result = await _unitOfWork.AiGeneratedQuestions.GetDetailByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.AiQuestionMessage.NOT_FOUND);

        return result.ToResponse();
    }

    public async Task<AiGeneratedQuestionResponse> EditAsync(string id, EditAiQuestionRequest request, string userId)
    {
        var aiQuestion = await _unitOfWork.AiGeneratedQuestions.GetByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.AiQuestionMessage.NOT_FOUND);

        if (aiQuestion.Status == AiQuestionStatus.Approved || aiQuestion.Status == AiQuestionStatus.Rejected)
            throw new AppException(MessageConstants.AiQuestionMessage.ALREADY_REVIEWED, 400);

        aiQuestion.GeneratedData = request.GeneratedData;
        aiQuestion.Status = AiQuestionStatus.Edited;
        aiQuestion.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.AiGeneratedQuestions.UpdateAsync(aiQuestion);
        await _unitOfWork.SaveChangesAsync();

        var result = await _unitOfWork.AiGeneratedQuestions.GetDetailByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.AiQuestionMessage.NOT_FOUND);

        return result.ToResponse();
    }
}
