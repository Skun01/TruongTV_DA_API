namespace Application.DTOs.GrammarCard;

public class UpdateGrammarCardRequest
{
    public string Term { set; get; } = string.Empty;
    public string Meaning { set; get; } = string.Empty;
    public string Structure { set; get; } = string.Empty;
    public string? Explanation { set; get; }
    public string? Caution { set; get; }
}
