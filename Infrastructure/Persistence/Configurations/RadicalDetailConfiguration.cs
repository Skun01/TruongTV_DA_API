using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RadicalDetailConfiguration : IEntityTypeConfiguration<RadicalDetail>
{
    public void Configure(EntityTypeBuilder<RadicalDetail> builder)
    {
        builder.ToTable("radical_details");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Character).IsRequired();
        builder.Property(r => r.MeaningVi).IsRequired();

        builder.HasIndex(r => r.Character).IsUnique();

        builder.HasOne(r => r.KanjiCard)
            .WithMany()
            .HasForeignKey(r => r.KanjiCardId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
