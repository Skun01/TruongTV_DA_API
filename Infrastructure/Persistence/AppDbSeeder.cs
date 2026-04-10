using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public static class AppDbSeeder
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (!await context.Database.CanConnectAsync(cancellationToken))
            return;

        const string ownerId = "seed-owner-001";
        const string ownerEmail = "seed.owner@learning.local";

        var owner = await context.Users.FirstOrDefaultAsync(u => u.Email == ownerEmail, cancellationToken);
        if (owner == null)
        {
            owner = new User
            {
                Id = ownerId,
                Username = "Seeder Owner",
                Email = ownerEmail,
                Role = UserRole.Admin,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
                IsVerified = true,
            };

            await context.Users.AddAsync(owner, cancellationToken);
        }
        else
        {
            owner.Role = UserRole.Admin;
            owner.IsVerified = true;
            owner.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123");
        }

        var hasVocabCards = await context.Cards.AnyAsync(c => c.CardType == CardType.Vocab, cancellationToken);
        if (!hasVocabCards)
        {
            var cards = new List<Card>
            {
                new()
                {
                    Id = "vocab-card-001",
                    CardType = CardType.Vocab,
                    Title = "食べる",
                    Summary = "Động từ ăn, dùng trong giao tiếp hằng ngày",
                    Level = JlptLevel.N5,
                    Tags = new List<string> { "dong-tu", "N5", "co-ban" },
                    Status = PublishStatus.Published,
                    CreatedBy = owner.Id,
                },
                new()
                {
                    Id = "vocab-card-002",
                    CardType = CardType.Vocab,
                    Title = "静か",
                    Summary = "Tính từ na chỉ trạng thái yên tĩnh",
                    Level = JlptLevel.N5,
                    Tags = new List<string> { "tinh-tu-na", "N5" },
                    Status = PublishStatus.Published,
                    CreatedBy = owner.Id,
                },
                new()
                {
                    Id = "vocab-card-003",
                    CardType = CardType.Vocab,
                    Title = "素晴らしい",
                    Summary = "Tính từ i mang nghĩa tuyệt vời",
                    Level = JlptLevel.N3,
                    Tags = new List<string> { "tinh-tu-i", "N3" },
                    Status = PublishStatus.Published,
                    CreatedBy = owner.Id,
                },
            };

            var details = new List<VocabularyDetail>
            {
                new()
                {
                    CardId = "vocab-card-001",
                    Writing = "食べる",
                    Reading = "たべる",
                    PitchAccent = "[1,0,0]",
                    AudioUrl = "https://example.com/audio/taberu.mp3",
                    WordType = WordType.Native,
                    Meanings =
                    [
                        new MeaningItem
                        {
                            PartOfSpeech = PartOfSpeech.VerbRu,
                            Definitions = new List<string> { "an", "dung bua" },
                        },
                    ],
                    Synonyms = new List<string> { "食う" },
                    Antonyms = new List<string> { "断食する" },
                    RelatedPhrases = new List<string> { "ご飯を食べる", "朝ごはんを食べる" },
                },
                new()
                {
                    CardId = "vocab-card-002",
                    Writing = "静か",
                    Reading = "しずか",
                    PitchAccent = "[0,1,0]",
                    AudioUrl = "https://example.com/audio/shizuka.mp3",
                    WordType = WordType.SinoJapanese,
                    Meanings =
                    [
                        new MeaningItem
                        {
                            PartOfSpeech = PartOfSpeech.NaAdj,
                            Definitions = new List<string> { "yen tinh", "im ang" },
                        },
                    ],
                    Synonyms = new List<string> { "穏やか" },
                    Antonyms = new List<string> { "うるさい" },
                    RelatedPhrases = new List<string> { "静かな部屋", "静かな場所" },
                },
                new()
                {
                    CardId = "vocab-card-003",
                    Writing = "素晴らしい",
                    Reading = "すばらしい",
                    PitchAccent = "[0,1,0,0,0]",
                    AudioUrl = "https://example.com/audio/subarashii.mp3",
                    WordType = WordType.SinoJapanese,
                    Meanings =
                    [
                        new MeaningItem
                        {
                            PartOfSpeech = PartOfSpeech.IAdj,
                            Definitions = new List<string> { "tuyet voi", "xuat sac" },
                        },
                    ],
                    Synonyms = new List<string> { "見事" },
                    Antonyms = new List<string> { "ひどい" },
                    RelatedPhrases = new List<string> { "素晴らしい景色", "素晴らしい作品" },
                },
            };

            var sentences = new List<Sentence>
            {
                new()
                {
                    Id = "sentence-001",
                    Text = "毎朝パンを食べる。",
                    Meaning = "Moi sang toi an banh mi.",
                    AudioUrl = "https://example.com/audio/s001.mp3",
                    Level = JlptLevel.N5,
                    CreatedBy = owner.Id,
                },
                new()
                {
                    Id = "sentence-002",
                    Text = "この部屋はとても静かです。",
                    Meaning = "Can phong nay rat yen tinh.",
                    AudioUrl = "https://example.com/audio/s002.mp3",
                    Level = JlptLevel.N5,
                    CreatedBy = owner.Id,
                },
                new()
                {
                    Id = "sentence-003",
                    Text = "彼の説明は素晴らしい。",
                    Meaning = "Phan giai thich cua anh ay rat tuyet voi.",
                    AudioUrl = "https://example.com/audio/s003.mp3",
                    Level = JlptLevel.N3,
                    CreatedBy = owner.Id,
                },
                new()
                {
                    Id = "sentence-004",
                    Text = "家で晩ご飯を食べる。",
                    Meaning = "An com toi o nha.",
                    AudioUrl = "https://example.com/audio/s004.mp3",
                    Level = JlptLevel.N5,
                    CreatedBy = owner.Id,
                },
            };

            var cardSentences = new List<CardSentence>
            {
                new() { CardId = "vocab-card-001", SentenceId = "sentence-001" },
                new() { CardId = "vocab-card-001", SentenceId = "sentence-004" },
                new() { CardId = "vocab-card-002", SentenceId = "sentence-002" },
                new() { CardId = "vocab-card-003", SentenceId = "sentence-003" },
            };

            await context.Cards.AddRangeAsync(cards, cancellationToken);
            await context.VocabularyDetails.AddRangeAsync(details, cancellationToken);
            await context.Sentences.AddRangeAsync(sentences, cancellationToken);
            await context.CardSentences.AddRangeAsync(cardSentences, cancellationToken);
        }

        var hasGrammarCards = await context.Cards.AnyAsync(c => c.CardType == CardType.Grammar, cancellationToken);
        if (!hasGrammarCards)
        {
            var grammarCards = new List<Card>
            {
                new()
                {
                    Id = "grammar-card-001",
                    CardType = CardType.Grammar,
                    Title = "〜てから",
                    Summary = "Hành động B diễn ra sau khi hoàn thành hành động A.",
                    Level = JlptLevel.N5,
                    Tags = new List<string> { "grammar", "sequence", "N5" },
                    Status = PublishStatus.Published,
                    CreatedBy = owner.Id,
                },
                new()
                {
                    Id = "grammar-card-002",
                    CardType = CardType.Grammar,
                    Title = "〜たあとで",
                    Summary = "Diễn tả hành động xảy ra sau một hành động khác.",
                    Level = JlptLevel.N4,
                    Tags = new List<string> { "grammar", "sequence", "N4" },
                    Status = PublishStatus.Published,
                    CreatedBy = owner.Id,
                },
                new()
                {
                    Id = "grammar-card-003",
                    CardType = CardType.Grammar,
                    Title = "〜ながら",
                    Summary = "Hai hành động xảy ra đồng thời, nhấn mạnh hành động chính.",
                    Level = JlptLevel.N4,
                    Tags = new List<string> { "grammar", "simultaneous", "N4" },
                    Status = PublishStatus.Draft,
                    CreatedBy = owner.Id,
                },
            };

            var grammarDetails = new List<GrammarDetail>
            {
                new()
                {
                    CardId = "grammar-card-001",
                    Structures = new List<GrammarStructureItem>
                    {
                        new()
                        {
                            Pattern = "**V[て形]** + {u}から{/u}",
                            Annotations = new Dictionary<string, string>
                            {
                                { "1", "Hành động trước hoàn tất rồi mới đến hành động sau." },
                            },
                        },
                        new() { Pattern = "*V[て形] + から、〜*" },
                    },
                    Explanation = "Dùng khi hành động sau chỉ xảy ra sau khi hành động trước đã hoàn tất. Có thể nhấn mạnh bằng {blue}ngữ cảnh thời gian{/blue}.",
                    Caution = "~~Không~~ dùng để diễn tả hai hành động đồng thời.",
                    Register = RegisterType.Standard,
                    AlternateForms = new List<string> { "〜てからです", "〜てからにする" },
                },
                new()
                {
                    CardId = "grammar-card-002",
                    Structures = new List<GrammarStructureItem>
                    {
                        new()
                        {
                            Pattern = "**V[た形]** + あとで",
                            Annotations = new Dictionary<string, string>
                            {
                                { "1", "Nhấn vào mốc hoàn tất của hành động đầu." },
                            },
                        },
                        new() { Pattern = "N + の + あとで" },
                    },
                    Explanation = "Nhấn vào thứ tự thời gian: sau khi A thì B.",
                    Caution = "Nên phân biệt sắc thái với 〜てから trong ngữ cảnh trang trọng.",
                    Register = RegisterType.Polite,
                    AlternateForms = new List<string> { "〜たあと" },
                },
                new()
                {
                    CardId = "grammar-card-003",
                    Structures = new List<GrammarStructureItem>
                    {
                        new()
                        {
                            Pattern = "V1(1) + ながら + V2(2)",
                            Annotations = new Dictionary<string, string>
                            {
                                { "1", "{green}Hành động phụ{/green} diễn ra đồng thời." },
                                { "2", "{purple}Hành động chính{/purple} cần nhấn mạnh." },
                            },
                        },
                    },
                    Explanation = "Vừa làm A vừa làm B, trong đó B là hành động chính.",
                    Caution = "Hai hành động phải do cùng chủ thể thực hiện.",
                    Register = RegisterType.Casual,
                    AlternateForms = new List<string> { "〜つつ" },
                },
            };

            var grammarRelations = new List<GrammarRelation>
            {
                new()
                {
                    GrammarId = "grammar-card-001",
                    RelatedId = "grammar-card-002",
                    RelationType = GrammarRelationType.Similar,
                },
                new()
                {
                    GrammarId = "grammar-card-001",
                    RelatedId = "grammar-card-003",
                    RelationType = GrammarRelationType.Contrasting,
                },
                new()
                {
                    GrammarId = "grammar-card-002",
                    RelatedId = "grammar-card-001",
                    RelationType = GrammarRelationType.Similar,
                },
            };

            var grammarResources = new List<GrammarResource>
            {
                new()
                {
                    Id = "grammar-resource-001",
                    CardId = "grammar-card-001",
                    Title = "Video giải thích 〜てから",
                    Url = "https://example.com/grammar/te-kara",
                },
                new()
                {
                    Id = "grammar-resource-002",
                    CardId = "grammar-card-001",
                    Title = "Bài tập 〜てから",
                    Url = "https://example.com/grammar/te-kara-exercises",
                },
                new()
                {
                    Id = "grammar-resource-003",
                    CardId = "grammar-card-002",
                    Title = "So sánh 〜たあとで và 〜てから",
                    Url = "https://example.com/grammar/ta-atode-vs-tekara",
                },
            };

            await context.Cards.AddRangeAsync(grammarCards, cancellationToken);
            await context.GrammarDetails.AddRangeAsync(grammarDetails, cancellationToken);
            await context.GrammarRelations.AddRangeAsync(grammarRelations, cancellationToken);
            await context.GrammarResources.AddRangeAsync(grammarResources, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
