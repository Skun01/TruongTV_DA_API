namespace Domain.Entities;

public class RadicalDetail : BaseEntity
{
    public string Character { get; set; } = string.Empty;
    public string MeaningVi { get; set; } = string.Empty;
    public string? KanjiCardId { get; set; }
    public Card? KanjiCard { get; set; }

    public ICollection<KanjiRadical> KanjiRadicals { get; set; } = new List<KanjiRadical>();
}
