using Application.DTOs.Internal;
using Application.DTOs.Resources;
using Application.IRepositories;
using Application.IServices;
using Application.IServices.IInternal;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class ResourceService : IResourceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileUploadService _fileUploadService;

    public ResourceService(IUnitOfWork unitOfWork, IFileUploadService fileUploadService)
    {
        _unitOfWork = unitOfWork;
        _fileUploadService = fileUploadService;
    }

    public async Task<UploadAudioResponse> UploadAudioAsync(string userId, UploadAudioRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        if (request.Content == Stream.Null || request.SizeInBytes <= 0 || string.IsNullOrWhiteSpace(request.FileName) || string.IsNullOrWhiteSpace(request.ContentType))
            throw new ApplicationException(MessageConstants.CommonMessage.INVALID);

        var uploadResult = await _fileUploadService.UploadAsync(new FileUploadRequest
        {
            UserId = userId,
            FileName = request.FileName,
            ContentType = request.ContentType,
            Content = request.Content,
            FileType = FileType.Audio,
            UsageType = ResourceUsageType.Audio,
        }, cancellationToken);

        var mediaAsset = new MediaAsset
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            FileUrl = uploadResult.FileUrl,
            StorageKey = uploadResult.StorageKey,
            OriginalFileName = request.FileName,
            ContentType = uploadResult.ContentType,
            SizeInBytes = request.SizeInBytes,
            FileType = uploadResult.FileType,
            UsageType = uploadResult.UsageType,
            StorageProvider = StorageProvider.Cloud,
        };

        await _unitOfWork.MediaAssets.AddAsync(mediaAsset);
        await _unitOfWork.SaveChangesAsync();

        return mediaAsset.ToUploadAudioResponse();
    }

    public async Task<UploadImageResponse> UploadImageAsync(string userId, UploadImageRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        if (request.Content == Stream.Null || request.SizeInBytes <= 0 || string.IsNullOrWhiteSpace(request.FileName) || string.IsNullOrWhiteSpace(request.ContentType))
            throw new ApplicationException(MessageConstants.CommonMessage.INVALID);

        var uploadResult = await _fileUploadService.UploadAsync(new FileUploadRequest
        {
            UserId = userId,
            FileName = request.FileName,
            ContentType = request.ContentType,
            Content = request.Content,
            FileType = FileType.Image,
            UsageType = ResourceUsageType.Image,
        }, cancellationToken);

        var mediaAsset = new MediaAsset
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            FileUrl = uploadResult.FileUrl,
            StorageKey = uploadResult.StorageKey,
            OriginalFileName = request.FileName,
            ContentType = uploadResult.ContentType,
            SizeInBytes = request.SizeInBytes,
            FileType = uploadResult.FileType,
            UsageType = uploadResult.UsageType,
            StorageProvider = StorageProvider.Cloud,
        };

        await _unitOfWork.MediaAssets.AddAsync(mediaAsset);
        await _unitOfWork.SaveChangesAsync();

        return mediaAsset.ToUploadImageResponse();
    }
}
