using Application.DTOs.Internal;

namespace Application.IServices.IInternal;

public interface IFileUploadService
{
    Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
