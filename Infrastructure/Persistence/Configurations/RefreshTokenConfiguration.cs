using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);
        
        builder.Property(rt => rt.Id)
            .HasMaxLength(50)
            .IsUnicode(false) 
            .ValueGeneratedNever();

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500) 
            .IsUnicode(false);

        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        builder.Property(rt => rt.Revoked)
            .HasDefaultValue(false);

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.HasOne(rt => rt.User)    
            .WithMany()               
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(rt => rt.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(rt => rt.UpdatedAt)
            .IsRequired(false);
    }
}
