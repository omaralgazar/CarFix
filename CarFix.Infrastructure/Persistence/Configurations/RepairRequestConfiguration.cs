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

            builder.HasOne(r => r.Vehicle)
                .WithMany(v => v.RepairRequests)
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}