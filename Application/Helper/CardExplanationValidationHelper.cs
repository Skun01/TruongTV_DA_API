using System.Text.Json;
using Application.DTOs.Cards;
using Domain.Constants;

namespace Application.Helper;

public static class CardExplanationValidationHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static CardExplanationContent ParseAndNormalize(string json)
    {
        var content = Deserialize(json);
        content.Answer = NormalizeRequired(content.Answer);

        return content;
    }

    private static CardExplanationContent Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<CardExplanationContent>(json, JsonOptions)
                ?? throw new ApplicationException(MessageConstants.CardMessage.AI_EXPLANATION_INVALID);
        }
        catch (JsonException)
        {
            throw new ApplicationException(MessageConstants.CardMessage.AI_EXPLANATION_INVALID);
        }
    }

    private static string NormalizeRequired(string value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ApplicationException(MessageConstants.CardMessage.AI_EXPLANATION_INVALID);

        return normalized;
    }
}
