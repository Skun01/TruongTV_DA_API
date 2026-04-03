using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SentenceConfiguration : IEntityTypeConfiguration<Sentence>
{
    public void Configure(EntityTypeBuilder<Sentence> builder)
    {
        builder.ToTable("sentences");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Text).IsRequired();
        builder.Property(s => s.Meaning).IsRequired();
    }
}
