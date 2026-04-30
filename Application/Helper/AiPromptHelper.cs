using Domain.Enums;

namespace Application.Helper;

/// <summary>
/// Tạo prompt cho AI sinh câu hỏi JLPT — trả về JSON chuẩn
/// </summary>
public static class AiPromptHelper
{
    private const string SystemPrompt = @"Bạn là chuyên gia soạn đề thi JLPT (Japanese Language Proficiency Test).
Nhiệm vụ: sinh câu hỏi theo đúng format JLPT thật, chất lượng cao, phù hợp cấp độ.
Luôn trả về JSON hợp lệ, không markdown, không giải thích thêm ngoài JSON.";

    public static string GetSystemPrompt() => SystemPrompt;

    public static string BuildPrompt(JlptLevel level, SectionType sectionType, string topic, int count)
    {
        return sectionType switch
        {
            SectionType.Moji => BuildMojiPrompt(level, topic, count),
            SectionType.Bunpou => BuildBunpouPrompt(level, topic, count),
            SectionType.Dokkai => BuildDokkaiPrompt(level, topic, count),
            SectionType.Choukai => BuildChoukaiPrompt(level, topic, count),
            _ => throw new ArgumentException($"SectionType không hợp lệ: {sectionType}")
        };
    }

    private static string BuildMojiPrompt(JlptLevel level, string topic, int count)
    {
        return $@"Tạo {count} câu hỏi phần 文字・語彙 (Moji/Goi - Từ vựng & Chữ Hán) cho kỳ thi JLPT {level}.
Chủ đề: {topic}

Yêu cầu:
- Mỗi câu có 1 câu tiếng Nhật chứa từ được gạch chân (đặt trong dấu ＿＿)
- 4 lựa chọn A, B, C, D — đúng 1 đáp án
- Dạng bài: đọc kanji, chọn nghĩa, chọn cách đọc, hoặc điền từ vựng phù hợp
- Giải thích ngắn bằng tiếng Việt
- Phù hợp trình độ {level}

Trả về JSON đúng format sau:
{{
  ""questions"": [
    {{
      ""questionText"": ""câu hỏi tiếng Nhật"",
      ""explanation"": ""giải thích tiếng Việt"",
      ""options"": [
        {{ ""label"": ""A"", ""text"": ""lựa chọn A"", ""isCorrect"": false }},
        {{ ""label"": ""B"", ""text"": ""lựa chọn B"", ""isCorrect"": true }},
        {{ ""label"": ""C"", ""text"": ""lựa chọn C"", ""isCorrect"": false }},
        {{ ""label"": ""D"", ""text"": ""lựa chọn D"", ""isCorrect"": false }}
      ]
    }}
  ]
}}";
    }

    private static string BuildBunpouPrompt(JlptLevel level, string topic, int count)
    {
        return $@"Tạo {count} câu hỏi phần 文法 (Bunpou - Ngữ pháp) cho kỳ thi JLPT {level}.
Chủ đề: {topic}

Yêu cầu:
- Dạng điền vào chỗ trống (＿＿＿) hoặc chọn cấu trúc ngữ pháp đúng
- 4 lựa chọn A, B, C, D — đúng 1 đáp án
- Câu hỏi phải dùng đúng mẫu ngữ pháp {level}
- Giải thích ngắn bằng tiếng Việt (nêu rõ mẫu ngữ pháp nào)

Trả về JSON đúng format sau:
{{
  ""questions"": [
    {{
      ""questionText"": ""câu hỏi tiếng Nhật có chỗ trống"",
      ""explanation"": ""giải thích mẫu ngữ pháp + tiếng Việt"",
      ""options"": [
        {{ ""label"": ""A"", ""text"": ""lựa chọn A"", ""isCorrect"": false }},
        {{ ""label"": ""B"", ""text"": ""lựa chọn B"", ""isCorrect"": true }},
        {{ ""label"": ""C"", ""text"": ""lựa chọn C"", ""isCorrect"": false }},
        {{ ""label"": ""D"", ""text"": ""lựa chọn D"", ""isCorrect"": false }}
      ]
    }}
  ]
}}";
    }

    private static string BuildDokkaiPrompt(JlptLevel level, string topic, int count)
    {
        var charCount = level switch
        {
            JlptLevel.N5 => "100-150",
            JlptLevel.N4 => "150-200",
            JlptLevel.N3 => "200-300",
            JlptLevel.N2 => "300-500",
            JlptLevel.N1 => "500-800",
            _ => "200-300"
        };

        return $@"Tạo 1 đoạn văn đọc hiểu tiếng Nhật (読解 - Dokkai) cho kỳ thi JLPT {level}, kèm {count} câu hỏi.
Chủ đề: {topic}

Yêu cầu:
- Đoạn văn khoảng {charCount} chữ tiếng Nhật, phù hợp trình độ {level}
- {count} câu hỏi đọc hiểu, mỗi câu 4 lựa chọn A, B, C, D
- Câu hỏi kiểm tra: hiểu nội dung chính, chi tiết, suy luận, ý nghĩa từ trong ngữ cảnh
- Giải thích ngắn bằng tiếng Việt

Trả về JSON đúng format sau:
{{
  ""passage"": ""đoạn văn tiếng Nhật"",
  ""questions"": [
    {{
      ""questionText"": ""câu hỏi về đoạn văn"",
      ""explanation"": ""giải thích tiếng Việt"",
      ""options"": [
        {{ ""label"": ""A"", ""text"": ""lựa chọn A"", ""isCorrect"": false }},
        {{ ""label"": ""B"", ""text"": ""lựa chọn B"", ""isCorrect"": true }},
        {{ ""label"": ""C"", ""text"": ""lựa chọn C"", ""isCorrect"": false }},
        {{ ""label"": ""D"", ""text"": ""lựa chọn D"", ""isCorrect"": false }}
      ]
    }}
  ]
}}";
    }

    private static string BuildChoukaiPrompt(JlptLevel level, string topic, int count)
    {
        return $@"Tạo 1 script hội thoại tiếng Nhật (聴解 - Choukai) cho kỳ thi JLPT {level}, kèm {count} câu hỏi.
Chủ đề: {topic}

Yêu cầu:
- Script hội thoại tự nhiên giữa 2-3 người, phù hợp trình độ {level}
- Hội thoại có tình huống rõ ràng (ở trường, công ty, cửa hàng, v.v.)
- {count} câu hỏi nghe hiểu, mỗi câu 4 lựa chọn A, B, C, D
- Câu hỏi kiểm tra: hiểu nội dung, mục đích, hành động tiếp theo
- Giải thích ngắn bằng tiếng Việt

Trả về JSON đúng format sau:
{{
  ""script"": ""script hội thoại tiếng Nhật (mỗi dòng ghi tên người nói: nội dung)"",
  ""questions"": [
    {{
      ""questionText"": ""câu hỏi về hội thoại"",
      ""explanation"": ""giải thích tiếng Việt"",
      ""options"": [
        {{ ""label"": ""A"", ""text"": ""lựa chọn A"", ""isCorrect"": false }},
        {{ ""label"": ""B"", ""text"": ""lựa chọn B"", ""isCorrect"": true }},
        {{ ""label"": ""C"", ""text"": ""lựa chọn C"", ""isCorrect"": false }},
        {{ ""label"": ""D"", ""text"": ""lựa chọn D"", ""isCorrect"": false }}
      ]
    }}
  ]
}}";
    }
}
