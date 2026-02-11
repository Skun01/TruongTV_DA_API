using System;

namespace Application.DTOs.ExampleSentence;

public class UpdateCardExampleRequest
{
    public string ClozeSentence { set; get; } = string.Empty;
    public string ExpectedAnswer { set; get; } = string.Empty;
    public string? Hint { set; get; }
}
