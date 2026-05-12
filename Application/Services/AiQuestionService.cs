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
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public AiQuestionService(IUnitOfWork unitOfWork, IAiGenerationService aiGenerationService)
    {
        _unitOfWork = unitOfWork;
        _aiGenerationService = aiGenerationService;
    }

    public async Task<List<AiGeneratedQuestionResponse>> GenerateQuestionsAsync(GenerateQuestionsRequest request, string userId)
    {
        var level = EnumParsingHelper.ParseRequired<JlptLevel>(request.Level);
        var sectionType = EnumParsingHelper.ParseRequired<SectionType>(request.SectionType);
        var topic = request.Topic.Trim();
        var targetGroup = await ValidateTargetGroupAsync(request.QuestionGroupId, level, sectionType);

        var generationResult = await _aiGenerationService.GenerateStructuredJsonAsync(
            AiPromptHelper.GetSystemPrompt(),
            AiPromptHelper.BuildPrompt(level, sectionType, topic, request.Count));

        var generatedJson = generationResult.Content;
        if (!JlptQuestionGenerationValidationHelper.TryParseAndValidate(
                generatedJson,
                level,
                sectionType,
                request.Count,
                out var parsedData,
                out var errors,
                out var warnings))
        {
            var repairResult = await _aiGenerationService.GenerateStructuredJsonAsync(
                AiPromptHelper.GetSystemPrompt(),
                AiPromptHelper.BuildRepairPrompt(level, sectionType, topic, request.Count, generatedJson, errors));

            generatedJson = repairResult.Content;

            if (!JlptQuestionGenerationValidationHelper.TryParseAndValidate(
                    generatedJson,
                    level,
                    sectionType,
                    request.Count,
                    out parsedData,
                    out errors,
                    out warnings))
            {
                throw new AppException(MessageConstants.AiQuestionMessage.INVALID_GENERATED_DATA, 400, details: errors);
            }
        }

        var generatedQuestions = new List<AiGeneratedQuestion>();

        if (parsedData?.Questions != null)
        {
            foreach (var questionItem in parsedData.Questions)
            {
                var singleQuestionData = new AiGeneratedQuestionData
                {
                    Passage = parsedData.Passage,
                    Script = parsedData.Script,
                    Difficulty = parsedData.Difficulty,
                    Questions = new List<AiGeneratedQuestionItem> { questionItem }
                };

                var duplicates = await GetDuplicateCandidatesAsync(level, sectionType, singleQuestionData);
                JlptQuestionGenerationValidationHelper.ApplyMetadata(singleQuestionData, level, sectionType, warnings, duplicates);

                var aiQuestion = new AiGeneratedQuestion
                {
                    Id = Guid.NewGuid().ToString(),
                    Level = level,
                    SectionType = sectionType,
                    Topic = topic,
                    QuestionGroupId = targetGroup?.Id,
                    GeneratedData = JsonSerializer.Serialize(singleQuestionData, JsonOptions),
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

        var parsedData = JlptQuestionGenerationValidationHelper.EnsureValidOrThrow(
            aiQuestion.GeneratedData,
            aiQuestion.Level,
            aiQuestion.SectionType,
            1);

        if (!string.IsNullOrWhiteSpace(aiQuestion.QuestionGroupId))
        {
            var createdQuestion = await CreateQuestionFromAiAsync(aiQuestion, parsedData);
            aiQuestion.QuestionId = createdQuestion.Id;
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

        var parsedData = JlptQuestionGenerationValidationHelper.EnsureValidOrThrow(
            request.GeneratedData,
            aiQuestion.Level,
            aiQuestion.SectionType,
            1);

        var duplicates = await GetDuplicateCandidatesAsync(aiQuestion.Level, aiQuestion.SectionType, parsedData);
        JlptQuestionGenerationValidationHelper.ApplyMetadata(parsedData, aiQuestion.Level, aiQuestion.SectionType, Array.Empty<string>(), duplicates);

        aiQuestion.GeneratedData = JlptQuestionGenerationValidationHelper.Serialize(parsedData);
        aiQuestion.Status = AiQuestionStatus.Edited;
        aiQuestion.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.AiGeneratedQuestions.UpdateAsync(aiQuestion);
        await _unitOfWork.SaveChangesAsync();

        var result = await _unitOfWork.AiGeneratedQuestions.GetDetailByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.AiQuestionMessage.NOT_FOUND);

        return result.ToResponse();
    }

    private async Task<QuestionGroup?> ValidateTargetGroupAsync(string? questionGroupId, JlptLevel level, SectionType sectionType)
    {
        if (string.IsNullOrWhiteSpace(questionGroupId))
            return null;

        var group = await _unitOfWork.QuestionGroups.GetByIdAsync(questionGroupId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.GROUP_NOT_FOUND);

        var section = await _unitOfWork.ExamSections.GetByIdAsync(group.SectionId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.SECTION_NOT_FOUND);

        var exam = await _unitOfWork.Exams.GetByIdAsync(section.ExamId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        if (section.SectionType != sectionType || exam.Level != level)
            throw new AppException(MessageConstants.AiQuestionMessage.TARGET_GROUP_MISMATCH, 400);

        if (exam.Status == PublishStatus.Published)
            throw new AppException(MessageConstants.ExamMessage.CANNOT_MODIFY_PUBLISHED, 400);

        return group;
    }

    private async Task<List<AiGeneratedQuestionDuplicateCandidate>> GetDuplicateCandidatesAsync(
        JlptLevel level,
        SectionType sectionType,
        AiGeneratedQuestionData data)
    {
        var existingQuestions = await _unitOfWork.Questions.GetDuplicateCandidatesAsync(level, sectionType);
        return JlptQuestionDuplicateHelper.FindDuplicates(data, existingQuestions);
    }

    private async Task<Question> CreateQuestionFromAiAsync(AiGeneratedQuestion aiQuestion, AiGeneratedQuestionData parsedData)
    {
        var groupId = aiQuestion.QuestionGroupId
            ?? throw new AppException(MessageConstants.AiQuestionMessage.TARGET_GROUP_MISMATCH, 400);

        var group = await _unitOfWork.QuestionGroups.GetWithQuestionsAsync(groupId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.GROUP_NOT_FOUND);

        var section = await _unitOfWork.ExamSections.GetByIdAsync(group.SectionId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.SECTION_NOT_FOUND);

        var exam = await _unitOfWork.Exams.GetByIdAsync(section.ExamId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        if (section.SectionType != aiQuestion.SectionType || exam.Level != aiQuestion.Level)
            throw new AppException(MessageConstants.AiQuestionMessage.TARGET_GROUP_MISMATCH, 400);

        if (exam.Status == PublishStatus.Published)
            throw new AppException(MessageConstants.ExamMessage.CANNOT_MODIFY_PUBLISHED, 400);

        var singleQuestion = parsedData.Questions.Single();
        var nextOrderIndex = group.Questions.Count == 0 ? 1 : group.Questions.Max(question => question.OrderIndex) + 1;

        if (aiQuestion.SectionType == SectionType.Dokkai && string.IsNullOrWhiteSpace(group.PassageText))
            group.PassageText = parsedData.Passage;

        if (aiQuestion.SectionType == SectionType.Choukai && string.IsNullOrWhiteSpace(group.AudioScript))
            group.AudioScript = parsedData.Script;

        group.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.QuestionGroups.UpdateAsync(group);

        var question = new Question
        {
            Id = Guid.NewGuid().ToString(),
            GroupId = group.Id,
            QuestionText = singleQuestion.QuestionText,
            Explanation = singleQuestion.Explanation,
            Score = 1,
            OrderIndex = nextOrderIndex,
        };

        foreach (var option in singleQuestion.Options)
        {
            var label = EnumParsingHelper.ParseRequired<OptionLabel>(option.Label);
            question.Options.Add(new QuestionOption
            {
                Id = Guid.NewGuid().ToString(),
                QuestionId = question.Id,
                Label = label,
                Text = option.Text,
                OptionType = OptionType.Text,
                IsCorrect = option.IsCorrect,
            });
        }

        await _unitOfWork.Questions.AddAsync(question);
        return question;
    }
}
