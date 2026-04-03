using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class NoteLikeConfiguration : IEntityTypeConfiguration<NoteLike>
{
    public void Configure(EntityTypeBuilder<NoteLike> builder)
    {
        builder.ToTable("note_likes");
        
        builder.HasKey(nl => new { nl.UserId, nl.NoteId });
        
        builder.HasOne(nl => nl.User)
            .WithMany()
            .HasForeignKey(nl => nl.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(nl => nl.Note)
            .WithMany(n => n.NoteLikes)
            .HasForeignKey(nl => nl.NoteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
