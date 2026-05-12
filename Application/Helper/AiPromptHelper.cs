using Domain.Enums;

namespace Application.Helper;

public static class AiPromptHelper
{
    public const string PromptVersion = "jlpt-ai-question-v2";

    private const string SystemPrompt = """
        Bạn là chuyên gia biên soạn câu hỏi JLPT cho người học tiếng Nhật.
        Mục tiêu là tạo câu hỏi giống phong cách đề thật, hợp level, đúng section, có distractor hợp lý và explanation ngắn bằng tiếng Việt.
        Luôn trả về JSON hợp lệ, không markdown, không thêm diễn giải ngoài JSON.
        Mỗi câu phải có đúng 4 lựa chọn A, B, C, D và chỉ đúng 1 đáp án.
        Nếu là Dokkai thì phải có passage.
        Nếu là Choukai thì phải có script hội thoại tự nhiên.
        Hãy tự kiểm tra trước khi trả kết quả để không sai schema.
        """;

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

    public static string BuildRepairPrompt(
        JlptLevel level,
        SectionType sectionType,
        string topic,
        int count,
        string invalidJson,
        IEnumerable<string> errors)
    {
        var errorText = string.Join("\n- ", errors.Where(error => !string.IsNullOrWhiteSpace(error)));

        return $"""
            JSON trước đó không hợp lệ hoặc không đạt yêu cầu business.
            Hãy sửa lại và trả về đúng JSON schema, không markdown.

            Các lỗi đã phát hiện:
            - {errorText}

            JSON lỗi:
            {invalidJson}

            {BuildPrompt(level, sectionType, topic, count)}
            """;
    }

    private static string BuildMojiPrompt(JlptLevel level, string topic, int count)
    {
        return $@"Tạo {count} câu hỏi phần 文字・語彙 cho JLPT {level}.
Chủ đề: {topic}

Yêu cầu:
- Phù hợp đúng level {level}.
- Dạng câu hỏi có thể là đọc kanji, chọn nghĩa, chọn cách đọc, hoặc điền từ phù hợp vào ngữ cảnh.
- Mỗi câu có đúng 4 lựa chọn A B C D và chỉ 1 đáp án đúng.
- Distractor phải hợp lý, không quá lộ.
- explanation viết tiếng Việt ngắn, nói rõ vì sao đáp án đúng.
- skillTags là mảng ngắn như vocabulary, kanji-reading, meaning, context.
- difficultyScore là số từ 0 đến 100.

Trả về đúng JSON:
{{
  ""difficulty"": {{
    ""level"": ""{level}"",
    ""score"": 0,
    ""reason"": ""string""
  }},
  ""questions"": [
    {{
      ""questionText"": ""string"",
      ""explanation"": ""string"",
      ""difficultyScore"": 0,
      ""skillTags"": [""string""],
      ""options"": [
        {{ ""label"": ""A"", ""text"": ""string"", ""isCorrect"": false }},
        {{ ""label"": ""B"", ""text"": ""string"", ""isCorrect"": true }},
        {{ ""label"": ""C"", ""text"": ""string"", ""isCorrect"": false }},
        {{ ""label"": ""D"", ""text"": ""string"", ""isCorrect"": false }}
      ]
    }}
  ]
}}";
    }

    private static string BuildBunpouPrompt(JlptLevel level, string topic, int count)
    {
        return $@"Tạo {count} câu hỏi phần 文法 cho JLPT {level}.
Chủ đề: {topic}

Yêu cầu:
- Dạng điền vào chỗ trống hoặc chọn mẫu ngữ pháp phù hợp.
- Phải dùng đúng mẫu ngữ pháp ở level {level}.
- 4 lựa chọn A B C D, chỉ 1 đáp án đúng.
- Các lựa chọn sai phải gần đúng để kiểm tra hiểu thật.
- explanation viết tiếng Việt ngắn, nêu rõ mẫu ngữ pháp và lý do đáp án đúng.
- skillTags là các nhãn như grammar, sentence-completion, nuance, contrast.
- difficultyScore là số từ 0 đến 100.

Trả về đúng JSON:
{{
  ""difficulty"": {{
    ""level"": ""{level}"",
    ""score"": 0,
    ""reason"": ""string""
  }},
  ""questions"": [
    {{
      ""questionText"": ""string"",
      ""explanation"": ""string"",
      ""difficultyScore"": 0,
      ""skillTags"": [""string""],
      ""options"": [
        {{ ""label"": ""A"", ""text"": ""string"", ""isCorrect"": false }},
        {{ ""label"": ""B"", ""text"": ""string"", ""isCorrect"": true }},
        {{ ""label"": ""C"", ""text"": ""string"", ""isCorrect"": false }},
        {{ ""label"": ""D"", ""text"": ""string"", ""isCorrect"": false }}
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

        return $@"Tạo 1 passage đọc hiểu tiếng Nhật cho JLPT {level}, kèm {count} câu hỏi.
Chủ đề: {topic}

Yêu cầu:
- passage dài khoảng {charCount} ký tự tiếng Nhật, phù hợp level {level}.
- {count} câu hỏi đọc hiểu, mỗi câu đúng 4 lựa chọn A B C D và chỉ 1 đáp án đúng.
- Câu hỏi nên phủ nhiều kỹ năng: main idea, detail, inference, vocabulary in context.
- explanation viết tiếng Việt ngắn, tập trung lý do đáp án đúng.
- difficultyScore là số từ 0 đến 100.

Trả về đúng JSON:
{{
  ""passage"": ""string"",
  ""difficulty"": {{
    ""level"": ""{level}"",
    ""score"": 0,
    ""reason"": ""string""
  }},
  ""questions"": [
    {{
      ""questionText"": ""string"",
      ""explanation"": ""string"",
      ""difficultyScore"": 0,
      ""skillTags"": [""main-idea""],
      ""options"": [
        {{ ""label"": ""A"", ""text"": ""string"", ""isCorrect"": false }},
        {{ ""label"": ""B"", ""text"": ""string"", ""isCorrect"": true }},
        {{ ""label"": ""C"", ""text"": ""string"", ""isCorrect"": false }},
        {{ ""label"": ""D"", ""text"": ""string"", ""isCorrect"": false }}
      ]
    }}
  ]
}}";
    }

    private static string BuildChoukaiPrompt(JlptLevel level, string topic, int count)
    {
        return $@"Tạo 1 script nghe hiểu tiếng Nhật cho JLPT {level}, kèm {count} câu hỏi.
Chủ đề: {topic}

Yêu cầu:
- script là hội thoại tự nhiên 2-3 người hoặc lời thông báo ngắn, phù hợp level {level}.
- script nên rõ tình huống, dễ dùng tiếp cho TTS.
- {count} câu hỏi nghe hiểu, mỗi câu đúng 4 lựa chọn A B C D và chỉ 1 đáp án đúng.
- Câu hỏi nên kiểm tra purpose, next action, detail, inference.
- explanation viết tiếng Việt ngắn.
- difficultyScore là số từ 0 đến 100.

Trả về đúng JSON:
{{
  ""script"": ""string"",
  ""difficulty"": {{
    ""level"": ""{level}"",
    ""score"": 0,
    ""reason"": ""string""
  }},
  ""questions"": [
    {{
      ""questionText"": ""string"",
      ""explanation"": ""string"",
      ""difficultyScore"": 0,
      ""skillTags"": [""listening-detail""],
      ""options"": [
        {{ ""label"": ""A"", ""text"": ""string"", ""isCorrect"": false }},
        {{ ""label"": ""B"", ""text"": ""string"", ""isCorrect"": true }},
        {{ ""label"": ""C"", ""text"": ""string"", ""isCorrect"": false }},
        {{ ""label"": ""D"", ""text"": ""string"", ""isCorrect"": false }}
      ]
    }}
  ]
}}";
    }
}
