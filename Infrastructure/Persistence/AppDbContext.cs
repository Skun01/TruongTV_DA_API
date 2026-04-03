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
    public DbSet<VocabularyDetail> VocabularyDetails { set; get; }
    public DbSet<Sentence> Sentences { set; get; }
    public DbSet<CardSentence> CardSentences { set; get; }
    public DbSet<UserCardNote> UserCardNotes { set; get; }
    public DbSet<NoteLike> NoteLikes { set; get; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
