using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CenterCapabilityConfig
    : IEntityTypeConfiguration<CenterCapability>
{
    public void Configure(EntityTypeBuilder<CenterCapability> builder)
    {
        builder.ToTable("CenterCapabilities");

        builder.HasKey(capability => capability.Id);

        builder.Property(capability => capability.IssueCategory)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(capability => capability.VehicleBrand)
            .HasMaxLength(100);

        builder.HasIndex(capability => new
        {
            capability.ServiceCenterId,
            capability.IssueCategory,
            capability.VehicleBrand
        });

        builder.HasOne(capability => capability.ServiceCenter)
            .WithMany(center => center.Capabilities)
            .HasForeignKey(capability => capability.ServiceCenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}