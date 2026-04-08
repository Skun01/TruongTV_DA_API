namespace Domain.Constants;

public static class VoicevoxConstants
{
    // Curated whitelist for Japanese learners: stable, clear, and easy-to-hear voices.
    public static readonly IReadOnlyList<int> RecommendedSpeakerIds = new List<int>
    {
        2,  // Shikoku Metan - Normal
        3,  // Zundamon - Normal
        8,  // Kasukabe Tsumugi - Normal
        10, // Amehare Hau - Normal
        11, // Kurono Takehiro - Normal
    };

    public static readonly HashSet<int> RecommendedSpeakerIdSet = new(RecommendedSpeakerIds);
}