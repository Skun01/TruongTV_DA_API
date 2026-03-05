using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DeckQueueConfiguration : IEntityTypeConfiguration<DeckQueue>
{
    public void Configure(EntityTypeBuilder<DeckQueue> builder)
    {
        builder.ToTable("DeckQueues");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UserId, x.DeckId }).IsUnique();

        builder.HasOne(q => q.User)
               .WithMany()
               .HasForeignKey(q => q.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(q => q.Deck)
               .WithMany()
               .HasForeignKey(q => q.DeckId)
               .OnDelete(DeleteBehavior.NoAction);
    }
}
