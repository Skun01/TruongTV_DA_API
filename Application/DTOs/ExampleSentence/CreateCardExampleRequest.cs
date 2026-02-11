namespace Application.DTOs.ExampleSentence;

public class CreateCardExampleRequest
{
    public string ClozeSentence { set; get; } = string.Empty;
    public string ExpectedAnswer { set; get; } = string.Empty;
    public string? Hint { set; get; }
    public string? VocabularyCardId { set; get; }
    public string? GrammarCardId { set; get; }
}
