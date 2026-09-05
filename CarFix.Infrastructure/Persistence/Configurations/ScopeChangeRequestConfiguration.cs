using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarFix.Infrastructure.Persistence.Configurations
{
    public class ScopeChangeRequestConfiguration : IEntityTypeConfiguration<ScopeChangeRequest>
    {
        public void Configure(EntityTypeBuilder<ScopeChangeRequest> builder)
        {
            builder.ToTable("ScopeChangeRequests");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Description).IsRequired().HasMaxLength(2000);
            builder.Property(s => s.ImageUrls).HasColumnType("text");
            builder.Property(s => s.ExtraCost).HasPrecision(18, 2).IsRequired();
            builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(30).IsRequired();

        }
    }
}