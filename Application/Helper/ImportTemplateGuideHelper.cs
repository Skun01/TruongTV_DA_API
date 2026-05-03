using Application.DTOs.Common;
using Domain.Constants;

namespace Application.Helper;

public static class ImportTemplateGuideHelper
{
    public static List<string> EnumValues<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetNames<TEnum>().ToList();
    }

    public static ImportTemplateGuide CreateBaseGuide()
    {
        return new ImportTemplateGuide
        {
            JsonNamingConvention = "camelCase",
            FieldNotes = new Dictionary<string, string>
            {
                ["items"] = "Danh sách bản ghi import.",
                ["rowNumber"] = "Số dòng trong file import, dùng để đối chiếu lỗi.",
            },
        };
    }
}
