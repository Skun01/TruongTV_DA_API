using Application.DTOs.Internal;
using Application.IServices.IInternal;
using Application.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
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
            throw new ApplicationException($"Cloudinary upload failed: {uploadResult.Error.Message}_500");
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

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return;
        }

        var deletionParams = new DeletionParams(storageKey)
        {
            Invalidate = true,
            ResourceType = ResourceType.Auto
        };

        var deleteResult = await _cloudinary.DestroyAsync(deletionParams);
        if (deleteResult.Error != null)
        {
            throw new ApplicationException($"Cloudinary delete failed: {deleteResult.Error.Message}_500");
        }

        _ = cancellationToken;
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.CloudName))
        {
            throw new ApplicationException("Cloudinary cloud name is not configured_500");
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new ApplicationException("Cloudinary api key is not configured_500");
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiSecret))
        {
            throw new ApplicationException("Cloudinary api secret is not configured_500");
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
            throw new ApplicationException("Invalid upload request_400");
        }
    }
}
