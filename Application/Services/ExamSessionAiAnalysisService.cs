using Application.Common;
using Application.DTOs.ExamSessions;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.IServices.IInternal;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ExamSessionAiAnalysisService : IExamSessionAiAnalysisService
{
    private static readonly TimeSpan AiTimeout = TimeSpan.FromSeconds(30);

    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiGenerationService _aiGenerationService;
    private readonly ILogger<ExamSessionAiAnalysisService> _logger;

    public ExamSessionAiAnalysisService(
        IUnitOfWork unitOfWork,
        IAiGenerationService aiGenerationService,
        ILogger<ExamSessionAiAnalysisService> logger)
    {
        _unitOfWork = unitOfWork;
        _aiGenerationService = aiGenerationService;
        _logger = logger;
    }

    public async Task<JlptAiAnalysisResponse> GetAsync(string sessionId, string userId)
    {
        var session = await LoadSubmittedSessionAsync(sessionId, userId);
        var input = JlptAiAnalysisInputHelper.BuildInput(session);
        var serializedInput = JlptAiAnalysisInputHelper.Serialize(input);
        var inputHash = JlptAiAnalysisInputHelper.ComputeInputHash(serializedInput);

        var cached = await _unitOfWork.ExamSessionAiAnalyses.GetLatestCompletedAsync(
            session.Id,
            JlptAiAnalysisPromptHelper.PromptVersion,
            inputHash);

        if (cached != null)
        {
            try
            {
                return cached.ToAiAnalysisResponse();
            }
            catch (ApplicationException ex) when (ex.Message == MessageConstants.ExamSessionMessage.AI_ANALYSIS_INVALID)
            {
                _logger.LogWarning(
                    "Cache AI analysis không hợp lệ cho session {SessionId}, sẽ generate lại",
                    session.Id);
            }
        }

        return await GenerateAndPersistAsync(session, input, inputHash, ExamSessionAiAnalysisTriggerType.AutoGenerate, null);
    }

    public async Task<JlptAiAnalysisResponse> RegenerateAsync(string sessionId, RegenerateJlptAiAnalysisRequest request, string userId)
    {
        var session = await LoadSubmittedSessionAsync(sessionId, userId);
        await EnsureRegenerationAllowedAsync(userId);

        var input = JlptAiAnalysisInputHelper.BuildInput(session);
        var serializedInput = JlptAiAnalysisInputHelper.Serialize(input);
        var inputHash = JlptAiAnalysisInputHelper.ComputeInputHash(serializedInput);

        return await GenerateAndPersistAsync(
            session,
            input,
            inputHash,
            ExamSessionAiAnalysisTriggerType.Regenerate,
            StringHelper.NormalizeOptional(request.Reason));
    }

    private async Task<ExamSession> LoadSubmittedSessionAsync(string sessionId, string userId)
    {
        var session = await _unitOfWork.ExamSessions.GetFullDetailAsync(sessionId)
            ?? throw new ApplicationException(MessageConstants.ExamSessionMessage.NOT_FOUND);

        if (session.UserId != userId)
            throw new AppException(MessageConstants.ExamSessionMessage.FORBIDDEN, 403);

        if (session.Status == ExamSessionStatus.InProgress
            || session.TotalScore == null
            || session.SectionScores.Count == 0)
        {
            throw new AppException(MessageConstants.ExamSessionMessage.NOT_SUBMITTED, 400);
        }

        return session;
    }

    private async Task EnsureRegenerationAllowedAsync(string userId)
    {
        var dayStartUtc = DateTime.UtcNow.Date;
        var regenerateCount = await _unitOfWork.ExamSessionAiAnalyses.CountByUserAndTriggerTypeSinceAsync(
            userId,
            ExamSessionAiAnalysisTriggerType.Regenerate,
            dayStartUtc);

        if (regenerateCount >= 3)
            throw new AppException(MessageConstants.ExamSessionMessage.AI_ANALYSIS_RATE_LIMITED, 429);
    }

    private async Task<JlptAiAnalysisResponse> GenerateAndPersistAsync(
        ExamSession session,
        JlptAiAnalysisInput input,
        string inputHash,
        ExamSessionAiAnalysisTriggerType triggerType,
        string? triggerReason)
    {
        using var cts = new CancellationTokenSource(AiTimeout);
        var startedAt = DateTime.UtcNow;
        var analysisId = Guid.NewGuid().ToString();

        try
        {
            var generation = await GenerateValidatedContentAsync(input, cts.Token);
            var response = generation.Content.ToAiAnalysisResponse(
                session,
                analysisId,
                generation.Model,
                JlptAiAnalysisPromptHelper.PromptVersion,
                startedAt);

            var entity = new ExamSessionAiAnalysis
            {
                Id = analysisId,
                ExamSessionId = session.Id,
                UserId = session.UserId,
                PromptVersion = JlptAiAnalysisPromptHelper.PromptVersion,
                Model = generation.Model,
                Status = ExamSessionAiAnalysisStatus.Completed,
                InputHash = inputHash,
                OutputJson = response.ToOutputJson(),
                LatencyMs = (int)(DateTime.UtcNow - startedAt).TotalMilliseconds,
                TriggerType = triggerType,
                TriggerReason = triggerReason,
                CreatedAt = startedAt,
            };

            await _unitOfWork.ExamSessionAiAnalyses.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return response;
        }
        catch (ApplicationException ex)
        {
            await PersistFailedAttemptAsync(session, inputHash, triggerType, triggerReason, ex.Message, startedAt);
            throw;
        }
    }

    private async Task<GenerationContentResult> GenerateValidatedContentAsync(JlptAiAnalysisInput input, CancellationToken cancellationToken)
    {
        var systemPrompt = JlptAiAnalysisPromptHelper.GetSystemPrompt();
        var userPrompt = JlptAiAnalysisPromptHelper.BuildUserPrompt(input);

        var initialResult = await _aiGenerationService.GenerateStructuredJsonAsync(systemPrompt, userPrompt, cancellationToken);
        var initialContent = TryParseContent(initialResult.Content, input);
        if (initialContent != null)
            return new GenerationContentResult(initialContent, initialResult.Model);

        _logger.LogWarning("AI analysis JSON không hợp lệ, thử repair một lần");

        var repairPrompt = JlptAiAnalysisPromptHelper.BuildRepairPrompt(input, initialResult.Content);
        var repairedResult = await _aiGenerationService.GenerateStructuredJsonAsync(systemPrompt, repairPrompt, cancellationToken);
        var repairedContent = TryParseContent(repairedResult.Content, input);

        if (repairedContent == null)
            throw new ApplicationException(MessageConstants.ExamSessionMessage.AI_ANALYSIS_INVALID);

        return new GenerationContentResult(repairedContent, repairedResult.Model);
    }

    private static JlptAiAnalysisContent? TryParseContent(string json, JlptAiAnalysisInput input)
    {
        try
        {
            return JlptAiAnalysisValidationHelper.ParseAndNormalize(json, input);
        }
        catch (ApplicationException ex) when (ex.Message == MessageConstants.ExamSessionMessage.AI_ANALYSIS_INVALID)
        {
            return null;
        }
    }

    private async Task PersistFailedAttemptAsync(
        ExamSession session,
        string inputHash,
        ExamSessionAiAnalysisTriggerType triggerType,
        string? triggerReason,
        string errorMessage,
        DateTime startedAt)
    {
        var entity = new ExamSessionAiAnalysis
        {
            Id = Guid.NewGuid().ToString(),
            ExamSessionId = session.Id,
            UserId = session.UserId,
            PromptVersion = JlptAiAnalysisPromptHelper.PromptVersion,
            Model = "unknown",
            Status = ExamSessionAiAnalysisStatus.Failed,
            InputHash = inputHash,
            OutputJson = string.Empty,
            ErrorMessage = SanitizeErrorMessage(errorMessage),
            LatencyMs = (int)(DateTime.UtcNow - startedAt).TotalMilliseconds,
            TriggerType = triggerType,
            TriggerReason = triggerReason,
            CreatedAt = startedAt,
        };

        await _unitOfWork.ExamSessionAiAnalyses.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    private static string SanitizeErrorMessage(string errorMessage)
    {
        return errorMessage switch
        {
            MessageConstants.ExamSessionMessage.AI_ANALYSIS_INVALID => errorMessage,
            MessageConstants.ExamSessionMessage.AI_ANALYSIS_UNAVAILABLE => errorMessage,
            _ => MessageConstants.ExamSessionMessage.AI_ANALYSIS_UNAVAILABLE
        };
    }

    private sealed record GenerationContentResult(JlptAiAnalysisContent Content, string Model);
}
