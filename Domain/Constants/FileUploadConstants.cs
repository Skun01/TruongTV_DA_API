namespace Domain.Constants;

public static class FileUploadConstants
{
    public static readonly string[] AllowedAvatarMimeTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    public static readonly string[] AllowedAudioMimeTypes =
    [
        "audio/mpeg",
        "audio/wav",
        "audio/mp4"
    ];
}