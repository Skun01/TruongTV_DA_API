using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("UserSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DailyGoal).IsRequired().HasDefaultValue(10);
        builder.Property(x => x.BatchSize).IsRequired().HasDefaultValue(5);
        builder.Property(x => x.CurrentStreak).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.LongestStreak).IsRequired().HasDefaultValue(0);

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasOne(s => s.User)
               .WithOne()
               .HasForeignKey<UserSettings>(s => s.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
