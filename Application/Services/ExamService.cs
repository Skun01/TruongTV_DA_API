using Application.Common;
using Application.DTOs.Exams;
using Application.DTOs.Internal;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.IServices.IInternal;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class ExamService : IExamService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITextToSpeechService _ttsService;
    private readonly IFileUploadService _fileUploadService;

    public ExamService(
        IUnitOfWork unitOfWork,
        ITextToSpeechService ttsService,
        IFileUploadService fileUploadService)
    {
        _unitOfWork = unitOfWork;
        _ttsService = ttsService;
        _fileUploadService = fileUploadService;
    }

    #region Exam CRUD

    public async Task<ExamDetailResponse> CreateExamAsync(CreateExamRequest request, string userId)
    {
        var level = EnumParsingHelper.ParseRequired<JlptLevel>(request.Level);

        var exam = new Exam
        {
            Id = Guid.NewGuid().ToString(),
            Title = request.Title.Trim(),
            Level = level,
            TotalDurationMinutes = request.TotalDurationMinutes,
            Status = PublishStatus.Draft,
            CreatedBy = userId,
        };

        await _unitOfWork.Exams.AddAsync(exam);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Exams.GetDetailByIdAsync(exam.Id)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        return created.ToDetailResponse();
    }

    public async Task<(List<ExamListItemResponse> Items, MetaData Meta)> SearchExamsAsync(ExamSearchQuery query)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);
        var level = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);
        var status = EnumParsingHelper.ParseNullable<PublishStatus>(query.Status);

        var (items, total) = await _unitOfWork.Exams.SearchAsync(
            query.Keyword,
            level,
            status,
            page,
            pageSize);

        return (
            items.Select(x => x.ToListItemResponse()).ToList(),
            new MetaData
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
            });
    }

    public async Task<(List<PublishedExamListItemResponse> Items, MetaData Meta)> SearchPublishedExamsAsync(PublishedExamQuery query)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);
        var level = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);

        var (items, total) = await _unitOfWork.Exams.SearchPublishedAsync(
            query.Keyword,
            level,
            page,
            pageSize);

        return (
            items.Select(x => x.ToPublishedListItemResponse()).ToList(),
            new MetaData
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
            });
    }

    public async Task<ExamDetailResponse> GetExamDetailAsync(string id)
    {
        var exam = await _unitOfWork.Exams.GetDetailByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        return exam.ToDetailResponse();
    }

    public async Task<PublishedExamDetailResponse> GetPublishedExamDetailAsync(string id)
    {
        var exam = await _unitOfWork.Exams.GetPublishedDetailByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        return exam.ToPublishedDetailResponse();
    }

    public async Task<ExamDetailResponse> UpdateExamAsync(string id, UpdateExamRequest request)
    {
        var exam = await _unitOfWork.Exams.GetByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        if (exam.Status == PublishStatus.Published)
            throw new AppException(MessageConstants.ExamMessage.CANNOT_MODIFY_PUBLISHED, 400);

        var level = EnumParsingHelper.ParseRequired<JlptLevel>(request.Level);

        exam.Title = request.Title.Trim();
        exam.Level = level;
        exam.TotalDurationMinutes = request.TotalDurationMinutes;
        exam.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Exams.UpdateAsync(exam);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Exams.GetDetailByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        return updated.ToDetailResponse();
    }

    public async Task PublishExamAsync(string id)
    {
        var exam = await _unitOfWork.Exams.GetDetailByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        if (exam.Status == PublishStatus.Published)
            throw new AppException(MessageConstants.ExamMessage.ALREADY_PUBLISHED, 400);

        if (exam.Sections.Count == 0)
            throw new AppException(MessageConstants.ExamMessage.NO_SECTIONS, 400);

        // Kiểm tra mỗi section phải có ít nhất 1 câu hỏi
        var hasEmptySection = exam.Sections.Any(s =>
            s.QuestionGroups.Count == 0 ||
            s.QuestionGroups.All(g => g.Questions.Count == 0));

        if (hasEmptySection)
            throw new AppException(MessageConstants.ExamMessage.NO_QUESTIONS, 400);

        // Cần refetch entity có tracking để update
        var examEntity = await _unitOfWork.Exams.GetByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        examEntity.Status = PublishStatus.Published;
        examEntity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Exams.UpdateAsync(examEntity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteExamAsync(string id)
    {
        var exam = await _unitOfWork.Exams.GetByIdAsync(id)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        if (exam.Status == PublishStatus.Published)
            throw new AppException(MessageConstants.ExamMessage.CANNOT_DELETE_PUBLISHED, 400);

        _unitOfWork.Exams.DeleteAsync(exam);
        await _unitOfWork.SaveChangesAsync();
    }

    #endregion

    #region Section

    public async Task<ExamSectionResponse> CreateSectionAsync(string examId, CreateSectionRequest request)
    {
        var exam = await _unitOfWork.Exams.GetByIdAsync(examId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        if (exam.Status == PublishStatus.Published)
            throw new AppException(MessageConstants.ExamMessage.CANNOT_MODIFY_PUBLISHED, 400);

        var sectionType = EnumParsingHelper.ParseRequired<SectionType>(request.SectionType);

        var section = new ExamSection
        {
            Id = Guid.NewGuid().ToString(),
            ExamId = examId,
            SectionType = sectionType,
            OrderIndex = request.OrderIndex,
            DurationMinutes = request.DurationMinutes,
            MaxScore = request.MaxScore,
            PassScore = request.PassScore,
        };

        await _unitOfWork.ExamSections.AddAsync(section);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.ExamSections.GetWithGroupsAsync(section.Id)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.SECTION_NOT_FOUND);

        return created.ToSectionResponse();
    }

    public async Task<ExamSectionResponse> UpdateSectionAsync(string examId, string sectionId, UpdateSectionRequest request)
    {
        var exam = await _unitOfWork.Exams.GetByIdAsync(examId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        if (exam.Status == PublishStatus.Published)
            throw new AppException(MessageConstants.ExamMessage.CANNOT_MODIFY_PUBLISHED, 400);

        var section = await _unitOfWork.ExamSections.GetByIdAsync(sectionId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.SECTION_NOT_FOUND);

        if (section.ExamId != examId)
            throw new ApplicationException(MessageConstants.ExamMessage.SECTION_NOT_FOUND);

        var sectionType = EnumParsingHelper.ParseRequired<SectionType>(request.SectionType);

        section.SectionType = sectionType;
        section.OrderIndex = request.OrderIndex;
        section.DurationMinutes = request.DurationMinutes;
        section.MaxScore = request.MaxScore;
        section.PassScore = request.PassScore;
        section.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.ExamSections.UpdateAsync(section);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.ExamSections.GetWithGroupsAsync(sectionId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.SECTION_NOT_FOUND);

        return updated.ToSectionResponse();
    }

    public async Task DeleteSectionAsync(string examId, string sectionId)
    {
        var exam = await _unitOfWork.Exams.GetByIdAsync(examId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        if (exam.Status == PublishStatus.Published)
            throw new AppException(MessageConstants.ExamMessage.CANNOT_MODIFY_PUBLISHED, 400);

        var section = await _unitOfWork.ExamSections.GetByIdAsync(sectionId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.SECTION_NOT_FOUND);

        if (section.ExamId != examId)
            throw new ApplicationException(MessageConstants.ExamMessage.SECTION_NOT_FOUND);

        _unitOfWork.ExamSections.DeleteAsync(section);
        await _unitOfWork.SaveChangesAsync();
    }

    #endregion

    #region QuestionGroup

    public async Task<QuestionGroupResponse> CreateQuestionGroupAsync(string sectionId, CreateQuestionGroupRequest request)
    {
        var section = await _unitOfWork.ExamSections.GetByIdAsync(sectionId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.SECTION_NOT_FOUND);

        // Kiểm tra exam chưa Published
        var exam = await _unitOfWork.Exams.GetByIdAsync(section.ExamId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        if (exam.Status == PublishStatus.Published)
            throw new AppException(MessageConstants.ExamMessage.CANNOT_MODIFY_PUBLISHED, 400);

        var mondaiType = EnumParsingHelper.ParseNullable<ChoukaiMondaiType>(request.MondaiType);

        var group = new QuestionGroup
        {
            Id = Guid.NewGuid().ToString(),
            SectionId = sectionId,
            PassageText = request.PassageText,
            AudioUrl = request.AudioUrl,
            AudioScript = request.AudioScript,
            Instruction = request.Instruction.Trim(),
            OrderIndex = request.OrderIndex,
            MondaiType = mondaiType,
        };

        await _unitOfWork.QuestionGroups.AddAsync(group);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.QuestionGroups.GetWithQuestionsAsync(group.Id)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.GROUP_NOT_FOUND);

        return created.ToGroupResponse();
    }

    public async Task<QuestionGroupResponse> UpdateQuestionGroupAsync(string sectionId, string groupId, UpdateQuestionGroupRequest request)
    {
        var section = await _unitOfWork.ExamSections.GetByIdAsync(sectionId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.SECTION_NOT_FOUND);

        var exam = await _unitOfWork.Exams.GetByIdAsync(section.ExamId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        if (exam.Status == PublishStatus.Published)
            throw new AppException(MessageConstants.ExamMessage.CANNOT_MODIFY_PUBLISHED, 400);

        var group = await _unitOfWork.QuestionGroups.GetByIdAsync(groupId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.GROUP_NOT_FOUND);

        if (group.SectionId != sectionId)
            throw new ApplicationException(MessageConstants.ExamMessage.GROUP_NOT_FOUND);

        var mondaiType = EnumParsingHelper.ParseNullable<ChoukaiMondaiType>(request.MondaiType);

        group.PassageText = request.PassageText;
        group.AudioUrl = request.AudioUrl;
        group.AudioScript = request.AudioScript;
        group.Instruction = request.Instruction.Trim();
        group.OrderIndex = request.OrderIndex;
        group.MondaiType = mondaiType;
        group.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.QuestionGroups.UpdateAsync(group);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.QuestionGroups.GetWithQuestionsAsync(groupId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.GROUP_NOT_FOUND);

        return updated.ToGroupResponse();
    }

    public async Task DeleteQuestionGroupAsync(string sectionId, string groupId)
    {
        var section = await _unitOfWork.ExamSections.GetByIdAsync(sectionId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.SECTION_NOT_FOUND);

        var exam = await _unitOfWork.Exams.GetByIdAsync(section.ExamId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        if (exam.Status == PublishStatus.Published)
            throw new AppException(MessageConstants.ExamMessage.CANNOT_MODIFY_PUBLISHED, 400);

        var group = await _unitOfWork.QuestionGroups.GetByIdAsync(groupId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.GROUP_NOT_FOUND);

        if (group.SectionId != sectionId)
            throw new ApplicationException(MessageConstants.ExamMessage.GROUP_NOT_FOUND);

        _unitOfWork.QuestionGroups.DeleteAsync(group);
        await _unitOfWork.SaveChangesAsync();
    }

    #endregion

    #region TTS — Sinh audio cho Choukai

    public async Task<QuestionGroupResponse> GenerateGroupAudioAsync(string groupId, string userId)
    {
        var group = await _unitOfWork.QuestionGroups.GetByIdAsync(groupId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.GROUP_NOT_FOUND);

        if (string.IsNullOrWhiteSpace(group.AudioScript))
            throw new AppException(MessageConstants.AiQuestionMessage.NO_AUDIO_SCRIPT, 400);

        // Kiểm tra exam chưa Published
        var section = await _unitOfWork.ExamSections.GetByIdAsync(group.SectionId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.SECTION_NOT_FOUND);

        var exam = await _unitOfWork.Exams.GetByIdAsync(section.ExamId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        if (exam.Status == PublishStatus.Published)
            throw new AppException(MessageConstants.ExamMessage.CANNOT_MODIFY_PUBLISHED, 400);

        // Gọi Azure TTS
        using var audioStream = await _ttsService.SynthesizeAsync(group.AudioScript);

        // Upload lên Cloudinary
        var uploadResult = await _fileUploadService.UploadAsync(new FileUploadRequest
        {
            UserId = userId,
            FileName = $"choukai_{groupId}.mp3",
            ContentType = "audio/mpeg",
            Content = audioStream,
            FileType = FileType.Audio,
            UsageType = ResourceUsageType.Audio,
        });

        // Cập nhật AudioUrl vào QuestionGroup
        group.AudioUrl = uploadResult.FileUrl;
        group.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.QuestionGroups.UpdateAsync(group);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.QuestionGroups.GetWithQuestionsAsync(groupId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.GROUP_NOT_FOUND);

        return updated.ToGroupResponse();
    }

    #endregion
}
