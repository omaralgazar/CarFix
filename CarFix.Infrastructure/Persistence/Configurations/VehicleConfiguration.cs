// VehicleConfiguration.cs
using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarFix.Infrastructure.Persistence.Configurations
{
    public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.ToTable("Vehicles");
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Brand).IsRequired().HasMaxLength(100);
            builder.Property(v => v.Model).IsRequired().HasMaxLength(100);
            builder.Property(v => v.LicensePlate).IsRequired().HasMaxLength(20);
            builder.Property(v => v.Year).IsRequired();
            builder.Property(v => v.IsDefault).IsRequired();
            builder.Property(v => v.IsDeleted).IsRequired();

            builder.HasOne(v => v.Customer)
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(v => !v.IsDeleted);
        }
    }
}