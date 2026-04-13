using Application.DTOs.Internal;
using Application.IServices.IInternal;
using Application.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.Constants;
using Microsoft.Extensions.Options;

namespace Infrastructure.InternalServices;

public class FileUploadService : IFileUploadService
{
    private readonly CloudinarySettings _settings;
    private readonly Cloudinary _cloudinary;

    public FileUploadService(IOptions<CloudinarySettings> options)
    {
        _settings = options.Value;
        ValidateSettings();

        var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var extension = Path.GetExtension(request.FileName) ?? string.Empty;
        var storageKey = BuildStorageKey(request.UserId, request.UsageType.ToString().ToLowerInvariant(), extension);
        var uploadParams = new AutoUploadParams
        {
            File = new FileDescription(request.FileName, request.Content),
            PublicId = storageKey,
            Folder = _settings.BaseFolder,
            UseFilename = false,
            UniqueFilename = false,
            Overwrite = true
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        if (uploadResult.Error != null)
        {
            throw new ApplicationException(MessageConstants.FileUploadMessage.CLOUDINARY_UPLOAD_FAILED);
        }

        var publicId = uploadResult.PublicId;

        return new FileUploadResult
        {
            StorageKey = publicId,
            FileUrl = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString() ?? string.Empty,
            ContentType = request.ContentType,
            SizeInBytes = request.Content.CanSeek ? request.Content.Length : 0,
            FileType = request.FileType,
            UsageType = request.UsageType
        };
    }

    public async Task DeleteAsync(string storageKey, Domain.Enums.FileType fileType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return;
        }

        var resourceType = fileType switch
        {
            Domain.Enums.FileType.Image => ResourceType.Image,
            Domain.Enums.FileType.Audio => ResourceType.Video,
            _ => ResourceType.Raw
        };

        var deletionParams = new DeletionParams(storageKey)
        {
            Invalidate = true,
            ResourceType = resourceType
        };

        var deleteResult = await _cloudinary.DestroyAsync(deletionParams);
        if (deleteResult.Error != null)
        {
            throw new ApplicationException(MessageConstants.FileUploadMessage.CLOUDINARY_DELETE_FAILED);
        }

        _ = cancellationToken;
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.CloudName))
        {
            throw new ApplicationException(MessageConstants.FileUploadMessage.CLOUDINARY_CLOUD_NAME_NOT_CONFIGURED);
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new ApplicationException(MessageConstants.FileUploadMessage.CLOUDINARY_API_KEY_NOT_CONFIGURED);
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiSecret))
        {
            throw new ApplicationException(MessageConstants.FileUploadMessage.CLOUDINARY_API_SECRET_NOT_CONFIGURED);
        }
    }

    private static string BuildStorageKey(string userId, string usage, string extension)
    {
        var now = DateTime.UtcNow;
        var safeExtension = extension.StartsWith('.') || string.IsNullOrWhiteSpace(extension)
            ? extension
            : $".{extension}";

        return $"{usage}/{userId}/{now:yyyy/MM}/{Guid.NewGuid():N}{safeExtension}";
    }

    private static void ValidateRequest(FileUploadRequest request)
    {
        if (request.Content == Stream.Null || string.IsNullOrWhiteSpace(request.FileName) || string.IsNullOrWhiteSpace(request.ContentType))
        {
            throw new ApplicationException(MessageConstants.FileUploadMessage.INVALID_UPLOAD_REQUEST);
        }
    }
}
