using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserCardNoteConfiguration : IEntityTypeConfiguration<UserCardNote>
{
    public void Configure(EntityTypeBuilder<UserCardNote> builder)
    {
        builder.ToTable("user_card_notes");
        
        builder.HasKey(n => n.Id);
        
        builder.Property(n => n.Content).IsRequired();
        
        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(n => n.Card)
            .WithMany(c => c.UserCardNotes)
            .HasForeignKey(n => n.CardId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Unique index
        builder.HasIndex(n => new { n.UserId, n.CardId }).IsUnique();
    }
}
