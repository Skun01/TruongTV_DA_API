using Domain.Enums;

namespace Domain.ValueObjects;

public class MeaningItem
{
    public PartOfSpeech PartOfSpeech { get; set; }
    public List<string> Definitions { get; set; } = new();
}
