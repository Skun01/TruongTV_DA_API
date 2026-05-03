namespace Application.DTOs.Vocabulary;

public class VocabularySentenceUpsertRequest
{
    public string? Id { get; set; }
    public int Position { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? Level { get; set; }
    public string? BlankWord { get; set; }
    public string? Hint { get; set; }
    public List<string> AnswerList { get; set; } = new();
}