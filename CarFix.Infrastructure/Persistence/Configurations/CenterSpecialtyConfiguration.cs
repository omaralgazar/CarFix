using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarFix.Infrastructure.Persistence.Configurations
{
    public class CenterSpecialtyConfiguration : IEntityTypeConfiguration<CenterSpecialty>
    {
        public void Configure(EntityTypeBuilder<CenterSpecialty> builder)
        {
            builder.ToTable("CenterSpecialties");
            builder.HasKey(cs => cs.Id);

            builder.Property(cs => cs.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(cs => cs.Value).IsRequired().HasMaxLength(100);

            builder.HasOne(cs => cs.ServiceCenter)
                .WithMany(sc => sc.Specialties)
                .HasForeignKey(cs => cs.ServiceCenterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(cs => new { cs.ServiceCenterId, cs.Type, cs.Value }).IsUnique();
        }
    }
}