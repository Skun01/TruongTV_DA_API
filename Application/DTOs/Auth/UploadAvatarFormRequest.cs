using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Auth;

public class UploadAvatarFormRequest
{
    public IFormFile Avatar { get; set; } = null!;
}