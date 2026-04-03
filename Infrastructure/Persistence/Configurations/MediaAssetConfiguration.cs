using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("MediaAssets");

        builder.HasKey(ma => ma.Id);

        builder.Property(ma => ma.Id)
            .HasMaxLength(50)
            .IsUnicode(false)
            .ValueGeneratedNever();

        builder.Property(ma => ma.UserId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ma => ma.FileUrl)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(ma => ma.StorageKey)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(ma => ma.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(ma => ma.ContentType)
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(ma => ma.SizeInBytes)
            .IsRequired();

        builder.Property(ma => ma.FileType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ma => ma.UsageType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ma => ma.StorageProvider)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ma => ma.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(ma => ma.UpdatedAt)
            .IsRequired(false);

        builder.HasIndex(ma => new { ma.UserId, ma.UsageType, ma.CreatedAt });

        builder.HasOne(ma => ma.User)
            .WithMany(u => u.MediaAssets)
            .HasForeignKey(ma => ma.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
