using Application.Common;
using Application.DTOs.ExamSessions;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class ExamSessionService : IExamSessionService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExamSessionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SessionStartResponse> StartSessionAsync(StartSessionRequest request, string userId)
    {
        var exam = await _unitOfWork.Exams.GetDetailByIdAsync(request.ExamId)
            ?? throw new ApplicationException(MessageConstants.ExamMessage.NOT_FOUND);

        if (exam.Status != PublishStatus.Published)
            throw new AppException(MessageConstants.ExamSessionMessage.EXAM_NOT_PUBLISHED, 400);

        var existingSession = await _unitOfWork.ExamSessions.GetActiveSessionByExamAsync(userId, request.ExamId);
        if (existingSession != null)
        {
            var savedAnswers = existingSession.Answers
                .ToDictionary(a => a.QuestionId, a => a.SelectedOptionId);

            return existingSession.ToStartResponse(savedAnswers);
        }

        var now = DateTime.UtcNow;

        var session = new ExamSession
        {
            Id = Guid.NewGuid().ToString(),
            ExamId = exam.Id,
            UserId = userId,
            StartedAt = now,
            ExpiresAt = now.AddMinutes(exam.TotalDurationMinutes),
            Status = ExamSessionStatus.InProgress,
        };

        await _unitOfWork.ExamSessions.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        session.Exam = exam;

        return session.ToStartResponse();
    }

    public async Task<SessionStartResponse> GetSessionAsync(string sessionId, string userId)
    {
        var session = await _unitOfWork.ExamSessions.GetFullDetailAsync(sessionId)
            ?? throw new ApplicationException(MessageConstants.ExamSessionMessage.NOT_FOUND);

        if (session.UserId != userId)
            throw new AppException(MessageConstants.ExamSessionMessage.FORBIDDEN, 403);

        // Nếu đã nộp bài thì không cần ẩn đáp án
        if (session.Status != ExamSessionStatus.InProgress)
            throw new AppException(MessageConstants.ExamSessionMessage.ALREADY_SUBMITTED, 400);

        // Tạo map đáp án đã lưu
        var savedAnswers = session.Answers
            .ToDictionary(a => a.QuestionId, a => a.SelectedOptionId);

        return session.ToStartResponse(savedAnswers);
    }

    public async Task<ActiveSessionLookupResponse> GetActiveSessionAsync(ActiveSessionQuery query, string userId)
    {
        var session = await _unitOfWork.ExamSessions.GetActiveSessionByExamAsync(userId, query.ExamId);
        return new ActiveSessionLookupResponse
        {
            HasActiveSession = session != null,
            SessionId = session?.Id,
        };
    }

    public async Task<SaveAnswerResponse> SaveAnswerAsync(string sessionId, SaveAnswerRequest request, string userId)
    {
        var session = await _unitOfWork.ExamSessions.GetWithAnswersAsync(sessionId)
            ?? throw new ApplicationException(MessageConstants.ExamSessionMessage.NOT_FOUND);

        if (session.UserId != userId)
            throw new AppException(MessageConstants.ExamSessionMessage.FORBIDDEN, 403);

        if (session.Status != ExamSessionStatus.InProgress)
            throw new AppException(MessageConstants.ExamSessionMessage.ALREADY_SUBMITTED, 400);

        var savedAt = DateTime.UtcNow;

        if (savedAt > session.ExpiresAt)
            throw new AppException(MessageConstants.ExamSessionMessage.SESSION_EXPIRED, 400);

        var question = await _unitOfWork.Questions.GetDetailByIdAsync(request.QuestionId);
        if (question == null)
            throw new AppException(MessageConstants.ExamSessionMessage.QUESTION_NOT_IN_EXAM, 400);

        var sectionBelongsToExam = session.Exam.Sections.Any(s => s.Id == question.Group.SectionId);
        if (!sectionBelongsToExam)
            throw new AppException(MessageConstants.ExamSessionMessage.QUESTION_NOT_IN_EXAM, 400);

        if (request.SelectedOptionId != null && question.Options.All(o => o.Id != request.SelectedOptionId))
            throw new AppException(MessageConstants.ExamSessionMessage.QUESTION_NOT_IN_EXAM, 400);

        var existingAnswer = session.Answers.FirstOrDefault(a => a.QuestionId == request.QuestionId);

        if (existingAnswer != null)
        {
            existingAnswer.SelectedOptionId = request.SelectedOptionId;
            existingAnswer.AnsweredAt = savedAt;
            existingAnswer.UpdatedAt = savedAt;
            _unitOfWork.SessionAnswers.UpdateAsync(existingAnswer);
        }
        else
        {
            var answer = new SessionAnswer
            {
                Id = Guid.NewGuid().ToString(),
                SessionId = sessionId,
                QuestionId = request.QuestionId,
                SelectedOptionId = request.SelectedOptionId,
                AnsweredAt = savedAt,
            };
            await _unitOfWork.SessionAnswers.AddAsync(answer);
        }

        await _unitOfWork.SaveChangesAsync();

        return new SaveAnswerResponse
        {
            QuestionId = request.QuestionId,
            SelectedOptionId = request.SelectedOptionId,
            SavedAt = savedAt,
        };
    }

    public async Task<SubmitSessionResponse> SubmitSessionAsync(string sessionId, string userId)
    {
        var session = await _unitOfWork.ExamSessions.GetFullDetailAsync(sessionId)
            ?? throw new ApplicationException(MessageConstants.ExamSessionMessage.NOT_FOUND);

        if (session.UserId != userId)
            throw new AppException(MessageConstants.ExamSessionMessage.FORBIDDEN, 403);

        if (session.Status != ExamSessionStatus.InProgress)
            throw new AppException(MessageConstants.ExamSessionMessage.ALREADY_SUBMITTED, 400);

        // Refetch session với tracking
        var sessionEntity = await _unitOfWork.ExamSessions.GetWithAnswersAsync(sessionId)
            ?? throw new ApplicationException(MessageConstants.ExamSessionMessage.NOT_FOUND);

        // Tính điểm
        var (totalScore, isPassed, sectionScores) = ExamScoringHelper.CalculateScore(sessionEntity, session.Exam);

        sessionEntity.TotalScore = totalScore;
        sessionEntity.IsPassed = isPassed;
        sessionEntity.Status = ExamSessionStatus.Submitted;
        sessionEntity.SubmittedAt = DateTime.UtcNow;
        sessionEntity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.ExamSessions.UpdateAsync(sessionEntity);

        // Lưu điểm từng section
        foreach (var sectionScore in sectionScores)
        {
            await _unitOfWork.SessionSectionScores.AddAsync(sectionScore);
        }

        await _unitOfWork.SaveChangesAsync();

        // Refetch để lấy đầy đủ data cho response
        var result = await _unitOfWork.ExamSessions.GetFullDetailAsync(sessionId)
            ?? throw new ApplicationException(MessageConstants.ExamSessionMessage.NOT_FOUND);

        return result.ToSubmitResponse();
    }

    public async Task<SessionResultResponse> GetSessionResultAsync(string sessionId, string userId)
    {
        var session = await _unitOfWork.ExamSessions.GetFullDetailAsync(sessionId)
            ?? throw new ApplicationException(MessageConstants.ExamSessionMessage.NOT_FOUND);

        if (session.UserId != userId)
            throw new AppException(MessageConstants.ExamSessionMessage.FORBIDDEN, 403);

        if (session.Status == ExamSessionStatus.InProgress)
            throw new AppException(MessageConstants.ExamSessionMessage.NOT_SUBMITTED, 400);

        return session.ToResultResponse();
    }

    public async Task<(List<SessionListItemResponse> Items, MetaData Meta)> GetSessionHistoryAsync(SessionHistoryQuery query, string userId)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);
        var status = EnumParsingHelper.ParseNullable<ExamSessionStatus>(query.Status);

        var (items, total) = await _unitOfWork.ExamSessions.SearchByUserAsync(
            userId,
            query.ExamId,
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
}
