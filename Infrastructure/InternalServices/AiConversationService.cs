using Application.Common;
using Application.DTOs.Ai;
using Application.IServices.IInternal;
using Domain.Constants;
using Microsoft.Extensions.Logging;

namespace Infrastructure.InternalServices;

public class AiConversationService : IAiConversationService
{
    private readonly IAiGenerationService _aiGenerationService;
    private readonly ILogger<AiConversationService> _logger;

    public AiConversationService(
        IAiGenerationService aiGenerationService,
        ILogger<AiConversationService> logger)
    {
        _aiGenerationService = aiGenerationService;
        _logger = logger;
    }

    public async Task<AiGeneratedJsonResult> GenerateConversationJsonAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _aiGenerationService.GenerateStructuredJsonAsync(systemPrompt, userPrompt, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "AI Conversation generation failed with AppException");
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "AI Conversation generation failed with ApplicationException");
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);
        }
    }
}
