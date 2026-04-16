using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DeckTypeConfiguration : IEntityTypeConfiguration<DeckType>
{
    public void Configure(EntityTypeBuilder<DeckType> builder)
    {
        builder.ToTable("deck_types");

        builder.HasKey(dt => dt.Id);

        builder.Property(dt => dt.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(dt => dt.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(dt => dt.Name)
            .IsUnique();
    }
}
