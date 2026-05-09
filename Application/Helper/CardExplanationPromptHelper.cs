using System.Text.Json;
using Domain.Entities;

namespace Application.Helper;

public static class CardExplanationPromptHelper
{
    public const string PromptVersion = "card-explanation-v3";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static string GetSystemPrompt()
    {
        return @"Bạn là trợ giảng tiếng Nhật cho người học Việt Nam.
Hãy lấy dữ liệu card làm nền tảng chính cho câu trả lời.
Bạn được phép bổ sung thông tin hữu ích như ví dụ, mẹo nhớ, lưu ý dùng, hoặc lỗi thường gặp ngay cả khi card không ghi sẵn, miễn là phần bổ sung đó hợp lý và không mâu thuẫn với dữ liệu card.
Không được tự ý đổi meaning, reading, kanji, grammar pattern, level, hoặc các fact cốt lõi nếu card đã cung cấp.
Trả lời đúng trọng tâm câu hỏi của người học, không tự mở rộng sang các phần họ không hỏi.
Nếu người học hỏi lỗi sai thường gặp thì chỉ tập trung vào lỗi sai thường gặp.
Nếu người học hỏi ví dụ thì ưu tiên đưa ví dụ.
Nếu người học hỏi cách dùng thì tập trung vào cách dùng.
Nếu người học hỏi card kanji và yêu cầu ví dụ/từ vựng, các ví dụ phải bám đúng kanji của card.
Viết tiếng Việt rõ, ngắn, thực dụng, tự nhiên.
Luôn trả về JSON hợp lệ, không markdown, không thêm chữ ngoài JSON.";
    }

    public static string BuildUserPrompt(Card card, string? userQuestion)
    {
        var context = BuildCardContext(card);
        var questionText = string.IsNullOrWhiteSpace(userQuestion)
            ? "Hãy giải thích card này để người học hiểu và nhớ tốt hơn."
            : userQuestion.Trim();

        return $@"Dữ liệu card:
{JsonSerializer.Serialize(context, JsonOptions)}

Câu hỏi của người học:
{questionText}

Yêu cầu:
- Chỉ trả lời đúng phần người học hỏi. Không tự chia thêm mục nếu không cần.
- Không lặp lại toàn bộ thông tin card nếu câu hỏi không cần.
- Dùng dữ liệu card làm điểm tựa chính, nhưng được phép bổ sung ví dụ hoặc mẹo nhớ hữu ích nếu phù hợp.
- Nếu bổ sung thông tin ngoài card, không được mâu thuẫn với dữ liệu card.
- Nếu câu hỏi yêu cầu ví dụ cho kanji, hãy dùng đúng kanji trong ví dụ.
- Câu trả lời có thể là 1 đoạn ngắn hoặc vài dòng ngắn, miễn là đúng trọng tâm.

Trả về đúng JSON object:
{{
  ""answer"": ""string""
}}";
    }

    public static string BuildRepairPrompt(Card card, string? userQuestion, string invalidJson)
    {
        return $@"JSON trước đó không hợp lệ hoặc thiếu field bắt buộc.
Hãy tạo lại đúng schema, không markdown.

Output lỗi:
{invalidJson}

{BuildUserPrompt(card, userQuestion)}";
    }

    private static object BuildCardContext(Card card)
    {
        return new
        {
            card.Id,
            CardType = card.CardType.ToString(),
            card.Title,
            card.Summary,
            Level = card.Level?.ToString(),
            card.Tags,
            Vocabulary = card.VocabularyDetail == null ? null : new
            {
                card.VocabularyDetail.Writing,
                card.VocabularyDetail.Reading,
                card.VocabularyDetail.PitchAccent,
                WordType = card.VocabularyDetail.WordType?.ToString(),
                Meanings = card.VocabularyDetail.Meanings.Select(meaning => new
                {
                    PartOfSpeech = meaning.PartOfSpeech.ToString(),
                    meaning.Definitions,
                }),
                card.VocabularyDetail.Synonyms,
                card.VocabularyDetail.Antonyms,
                card.VocabularyDetail.RelatedPhrases,
            },
            Grammar = card.GrammarDetail == null ? null : new
            {
                Structures = card.GrammarDetail.Structures.Select(structure => new
                {
                    structure.Pattern,
                    structure.Annotations,
                }),
                card.GrammarDetail.Explanation,
                card.GrammarDetail.Caution,
                Register = card.GrammarDetail.Register?.ToString(),
                card.GrammarDetail.AlternateForms,
                Resources = card.GrammarResources.Select(resource => new
                {
                    resource.Title,
                    resource.Url,
                }),
            },
            Kanji = card.KanjiDetail == null ? null : new
            {
                card.KanjiDetail.Kanji,
                card.KanjiDetail.StrokeCount,
                card.KanjiDetail.Onyomi,
                card.KanjiDetail.Kunyomi,
                card.KanjiDetail.HanViet,
                card.KanjiDetail.MeaningVi,
                Radicals = card.KanjiRadicals.Select(kanjiRadical => new
                {
                    kanjiRadical.Radical.Character,
                    kanjiRadical.Radical.MeaningVi,
                }),
            },
            Sentences = card.CardSentences
                .OrderBy(cardSentence => cardSentence.Position)
                .Select(cardSentence => new
                {
                    cardSentence.Sentence.Text,
                    cardSentence.Sentence.Meaning,
                    cardSentence.BlankWord,
                    cardSentence.Hint,
                    cardSentence.AnswerList,
                }),
        };
    }
}
