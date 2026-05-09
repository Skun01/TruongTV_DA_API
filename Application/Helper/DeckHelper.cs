using Domain.Enums;

namespace Application.Helper;

public static class DeckHelper
{
    public static DeckVisibility ParseVisibilityOrDefault(string? value, DeckVisibility defaultValue)
    {
        return EnumParsingHelper.ParseNullable<DeckVisibility>(value) ?? defaultValue;
    }

    public static PublishStatus ParseStatusOrDefault(string? value, PublishStatus defaultValue)
    {
        return EnumParsingHelper.ParseNullable<PublishStatus>(value) ?? defaultValue;
    }
}
