namespace Application.DTOs.Internal;

/// <summary>
/// Kết quả tổng hợp audio bằng Azure TTS và upload lên Cloudinary.
/// </summary>
public class AudioSynthesisResult
{
    public string AudioUrl { get; set; } = string.Empty;
}