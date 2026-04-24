using Application.DTOs.Internal;

namespace Application.IServices.IInternal;

public interface IPronunciationAssessmentService
{
    Task<PronunciationAssessmentResult> AssessAsync(
        Stream audioStream,
        string contentType,
        string referenceText,
        string locale,
        CancellationToken cancellationToken = default);
}
