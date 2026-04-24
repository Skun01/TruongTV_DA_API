using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Shadowing;

public class SubmitShadowingAttemptFormRequest
{
    public string TopicId { get; set; } = string.Empty;
    public string SentenceId { get; set; } = string.Empty;
    public string? Locale { get; set; }
    public IFormFile Audio { get; set; } = null!;
}
