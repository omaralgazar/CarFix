using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarFix.Infrastructure.Persistence.Configurations
{
    public class RepairOrderConfiguration : IEntityTypeConfiguration<RepairOrder>
    {
        public void Configure(EntityTypeBuilder<RepairOrder> builder)
        {
            builder.ToTable("RepairOrders");
            builder.HasKey(o => o.Id);

            builder.Property(o => o.CheckInOTP).HasMaxLength(4);
            builder.Property(o => o.CheckOutOTP).HasMaxLength(4);
            builder.Property(o => o.OriginalDurationInHours).IsRequired();
            builder.Property(o => o.ExtendedDurationInHours).IsRequired();
            builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(30).IsRequired();

            builder.HasOne(o => o.RepairRequest)
                .WithOne(r => r.Order)
                .HasForeignKey<RepairOrder>(o => o.RepairRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.AcceptedOffer)
                .WithMany()
                .HasForeignKey(o => o.AcceptedOfferId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.ScopeChangeRequests)
                .WithOne(s => s.RepairOrder)
                .HasForeignKey(s => s.RepairOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.Disputes)
                .WithOne(d => d.Order)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Invoice)
                .WithOne(i => i.Order)
                .HasForeignKey<Invoice>(i => i.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Review)
                .WithOne(r => r.RepairOrder)
                .HasForeignKey<Review>(r => r.RepairOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}