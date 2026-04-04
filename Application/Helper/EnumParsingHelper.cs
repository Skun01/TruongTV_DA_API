using Domain.Constants;

namespace Application.Helper;

public static class EnumParsingHelper
{
    public static TEnum? ParseNullable<TEnum>(string? value) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (Enum.TryParse<TEnum>(value.Trim(), true, out var parsed))
            return parsed;

        throw new ApplicationException(MessageConstants.CommonMessage.INVALID);
    }

    public static TEnum ParseRequired<TEnum>(string value) where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(value.Trim(), true, out var parsed))
            return parsed;

        throw new ApplicationException(MessageConstants.CommonMessage.INVALID);
    }
}
