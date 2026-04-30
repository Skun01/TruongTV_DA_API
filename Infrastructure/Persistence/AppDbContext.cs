using System.Reflection;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users { set; get; }
    public DbSet<RefreshToken> RefreshTokens { set; get; }
    public DbSet<MediaAsset> MediaAssets { set; get; }
    
    public DbSet<Card> Cards { set; get; }
    public DbSet<DeckType> DeckTypes { set; get; }
    public DbSet<Deck> Decks { set; get; }
    public DbSet<DeckFolder> DeckFolders { set; get; }
    public DbSet<FolderCard> FolderCards { set; get; }
    public DbSet<DeckBookmark> DeckBookmarks { set; get; }
    public DbSet<VocabularyDetail> VocabularyDetails { set; get; }
    public DbSet<GrammarDetail> GrammarDetails { set; get; }
    public DbSet<KanjiDetail> KanjiDetails { set; get; }
    public DbSet<RadicalDetail> RadicalDetails { set; get; }
    public DbSet<KanjiRadical> KanjiRadicals { set; get; }
    public DbSet<GrammarRelation> GrammarRelations { set; get; }
    public DbSet<GrammarResource> GrammarResources { set; get; }
    public DbSet<Sentence> Sentences { set; get; }
    public DbSet<CardSentence> CardSentences { set; get; }
    public DbSet<UserCardProgress> UserCardProgresses { set; get; }
    public DbSet<StudySession> StudySessions { set; get; }
    public DbSet<UserLearningSettings> UserLearningSettings { set; get; }
    public DbSet<UserCardNote> UserCardNotes { set; get; }
    public DbSet<NoteLike> NoteLikes { set; get; }
    public DbSet<ShadowingTopic> ShadowingTopics { set; get; }
    public DbSet<ShadowingTopicSentence> ShadowingTopicSentences { set; get; }
    public DbSet<ShadowingAttempt> ShadowingAttempts { set; get; }

    // JLPT Exam module
    public DbSet<Exam> Exams { set; get; }
    public DbSet<ExamSection> ExamSections { set; get; }
    public DbSet<QuestionGroup> QuestionGroups { set; get; }
    public DbSet<Question> Questions { set; get; }
    public DbSet<QuestionOption> QuestionOptions { set; get; }
    public DbSet<ExamSession> ExamSessions { set; get; }
    public DbSet<SessionAnswer> SessionAnswers { set; get; }
    public DbSet<SessionSectionScore> SessionSectionScores { set; get; }
    public DbSet<AiGeneratedQuestion> AiGeneratedQuestions { set; get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
