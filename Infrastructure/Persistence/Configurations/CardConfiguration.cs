using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.ToTable("cards");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Title).IsRequired();
        
        builder.Property(c => c.CardType).IsRequired();
        
        builder.HasOne(c => c.Creator)
            .WithMany()
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);
            
        // Tags array will be handled natively by Npgsql as text[] automatically
    }
}
