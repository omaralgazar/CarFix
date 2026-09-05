using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarFix.Infrastructure.Persistence.Configurations
{
    public class RepairOfferConfiguration : IEntityTypeConfiguration<RepairOffer>
    {
        public void Configure(EntityTypeBuilder<RepairOffer> builder)
        {
            builder.ToTable("RepairOffers");
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Cost).HasPrecision(18, 2).IsRequired();
            builder.Property(o => o.DurationInHours).IsRequired();
            builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(30).IsRequired();

            builder.Ignore(o => o.GracePeriodHours);

            builder.HasOne(o => o.Request)
                .WithMany(r => r.Offers)
                .HasForeignKey(o => o.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Center)
                .WithMany(sc => sc.Offers)
                .HasForeignKey(o => o.CenterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}