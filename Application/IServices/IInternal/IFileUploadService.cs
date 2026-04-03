using Application.DTOs.Internal;
using Domain.Enums;
namespace Application.IServices.IInternal;

public interface IFileUploadService
{
    Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, FileType fileType, CancellationToken cancellationToken = default);
}
