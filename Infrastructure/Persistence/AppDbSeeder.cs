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

        var hasVocabCards = await context.Cards.AnyAsync(c => c.CardType == CardType.Vocab, cancellationToken);
        if (hasVocabCards)
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
                Role = UserRole.User,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
                IsVerified = true,
            };

            await context.Users.AddAsync(owner, cancellationToken);
        }

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
            },
            new()
            {
                Id = "sentence-002",
                Text = "この部屋はとても静かです。",
                Meaning = "Can phong nay rat yen tinh.",
                AudioUrl = "https://example.com/audio/s002.mp3",
                Level = JlptLevel.N5,
            },
            new()
            {
                Id = "sentence-003",
                Text = "彼の説明は素晴らしい。",
                Meaning = "Phan giai thich cua anh ay rat tuyet voi.",
                AudioUrl = "https://example.com/audio/s003.mp3",
                Level = JlptLevel.N3,
            },
            new()
            {
                Id = "sentence-004",
                Text = "家で晩ご飯を食べる。",
                Meaning = "An com toi o nha.",
                AudioUrl = "https://example.com/audio/s004.mp3",
                Level = JlptLevel.N5,
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
        await context.SaveChangesAsync(cancellationToken);
    }
}
