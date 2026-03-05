using System.Reflection;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users { set; get; }
    public DbSet<RefreshToken> RefreshTokens { set; get; }
    public DbSet<Deck> Decks { set; get; }
    public DbSet<VocabularyCard> VocabularyCards { set; get; }
    public DbSet<GrammarCard> GrammarCards { set; get; }
    public DbSet<ExampleSentence> ExampleSentences { set; get; }
    public DbSet<UserSettings> UserSettings { set; get; }
    public DbSet<DeckQueue> DeckQueues { set; get; }
    public DbSet<CardProgress> CardProgresses { set; get; }
    public DbSet<ReviewLog> ReviewLogs { set; get; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
