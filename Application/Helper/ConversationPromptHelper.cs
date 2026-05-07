using Domain.Enums;
using Domain.Entities;

namespace Application.Helper;

public static class ConversationPromptHelper
{
    public const string PromptVersion = "conversation-v2";

    private const string SystemPrompt = @"Bạn là người bản xứ Nhật Bản, trẻ trung, thân thiện.
Nhiệm vụ: hội thoại tiếng Nhật với user theo kịch bản thực tế.
LUÔN điều chỉnh độ khó theo JLPT level:
- N5: câu ngắn (5-10 chữ), từ vựng cơ bản hiragana/katakana, không kanji phức tạp
- N4: câu đơn giản (10-15 chữ), từ vựng khoảng 1500 từ common
- N3: câu phức tạp hơn (15-20 chữ), từ vựng trung bình
- N2: câu phức tạp (20-25 chữ), idioms, formal language
- N1: câu văn phong cao (25+ chữ), slang, keigo

LUẬT NGHIÊM NGẶT:
1. Phản hồi NGẮN GỌN (1-3 câu), tự nhiên như người bản xứ
2. Gợi ý 2-3 câu user có thể nói tiếp theo (Suggestions) - viết bằng tiếng Nhật
3. Nếu user sử dụng từ mới hoặc cấu trúc ngữ pháp hay → trích xuất vào newVocabulary
4. Nếu phát hiện lỗi ngữ pháp → trích xuất vào grammarPoints
5. Phản hồi CHỈ JSON, không markdown, không giải thích:

{
  ""text"": ""câu trả lời tiếng Nhật"",
  ""suggestions"": [""câu gợi ý 1"", ""câu gợi ý 2"", ""câu gợi ý 3""],
  ""newVocabulary"": [
    {""word"": ""từ mới"", ""reading"": ""cách đọc"", ""meaning"": ""nghĩa tiếng Việt"", ""example"": ""ví dụ"", ""jlptLevel"": ""N5""}
  ],
  ""grammarPoints"": [""～たい form"", ""～てください""]
}";

    public static string GetSystemPrompt() => SystemPrompt;

    public static string BuildStartPrompt(string scenarioText, JlptLevel level)
    {
        return $@"Kịch bản: {scenarioText}
Mức độ: JLPT {level}

Bắt đầu cuộc hội thoại! Bạn là người Nhật bản xứ. Hãy chào hỏi và bắt đầu tình huống một cách tự nhiên.
Phản hồi JSON theo format quy định.";
    }

    public static string BuildContinuePrompt(
        string scenarioText,
        JlptLevel level,
        string conversationHistory,
        string userMessage)
    {
        return $@"Kịch bản: {scenarioText}
Mức độ: JLPT {level}

Lịch sử hội thoại:
{conversationHistory}

User vừa nói: ""{userMessage}""

Hãy phản hồi tự nhiên như người bản xứ Nhật Bản.
Phản hồi JSON theo format quy định.";
    }

    public static string BuildEndPrompt(
        string scenarioText,
        JlptLevel level,
        string conversationHistory,
        int totalMessages,
        int userMessagesCount)
    {
        return $@"Kịch bản: {scenarioText}
Mức độ: JLPT {level}

Lịch sử hội thoại:
{conversationHistory}

Cuộc hội thoại đã kết thúc (tổng {totalMessages} tin nhắn, {userMessagesCount} tin nhắn từ user).

Hãy đánh giá và đưa ra feedback bằng tiếng Việt:
- Tổng quan mức độ hoàn thành
- Điểm mạnh
- Cần cải thiện
- Điểm số 0-100

Trả về JSON format:
{{
  ""feedback"": ""feedback tiếng Việt"",
  ""score"": 0-100
}}";
    }

    public static string ResolveScenarioText(string scenario, string? customScenario = null)
    {
        return scenario.Equals("Custom", StringComparison.OrdinalIgnoreCase)
            ? customScenario?.Trim() ?? "Tự chọn kịch bản"
            : GetScenarioDescription(scenario);
    }

    public static string BuildConversationHistory(IEnumerable<ConversationMessage> messages)
    {
        return string.Join(
            "\n",
            messages
                .OrderBy(x => x.CreatedAt)
                .Select(x => $"{(x.Sender == MessageSender.User ? "User" : "AI")}: {x.Text}"));
    }

    private static string GetScenarioDescription(string scenario)
    {
        return scenario switch
        {
            "Shopping" => "Đi shopping ở cửa hàng quần áo/mua sắm",
            "Interview" => "Phỏng vấn xin việc",
            "Direction" => "Hỏi đường ở Nhật Bản",
            "Meeting" => "Gặp gỡ bạn mới",
            "Restaurant" => "Nhà hàng/quán ăn",
            _ => scenario
        };
    }
}
