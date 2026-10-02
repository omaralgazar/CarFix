using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarFix.Infrastructure.Persistence.Configurations
{
    public class RepairRequestConfiguration : IEntityTypeConfiguration<RepairRequest>
    {
        public void Configure(EntityTypeBuilder<RepairRequest> builder)
        {
            builder.ToTable("RepairRequests");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.IssueCategory).IsRequired().HasMaxLength(100);
            builder.Property(r => r.IssueDescription).IsRequired().HasMaxLength(2000);
            builder.Property(r => r.ImageUrls).HasColumnType("text");
            builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(30).IsRequired();

            builder.HasOne(r => r.Customer)
                .WithMany(u => u.RepairRequests)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(request => request.FulfillmentMethod)
                    .HasConversion<string>()
                    .HasMaxLength(30)
                    .IsRequired();

            builder.Property(request => request.PickupContactName)
                .HasMaxLength(100);

            builder.Property(request => request.PickupContactPhone)
                .HasMaxLength(20);

            builder.Property(request => request.PickupAddressSnapshot)
                .HasMaxLength(500);

            builder.Property(request => request.PickupLatitude)
                .HasPrecision(9, 6);

            builder.Property(request => request.PickupLongitude)
                .HasPrecision(9, 6);

            builder.HasOne(request => request.PickupAddress)
                .WithMany()
                .HasForeignKey(request => request.PickupAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Vehicle)
                .WithMany(v => v.RepairRequests)
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}