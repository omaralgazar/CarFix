using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using CarFix.Domain.Entities;

namespace CarFix.Infrastructure.Persistence.Configurations
{
    public class ServiceCenterConfiguration : IEntityTypeConfiguration<ServiceCenter>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<CarFix.Domain.Entities.ServiceCenter> builder)
        {
            builder.ToTable("ServiceCenters");
            builder.HasKey(sc => sc.Id);
            builder.Property(sc => sc.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(sc => sc.OwnerUserId)
                .IsRequired();

            builder.Property(center => center.LogoUrl)
                 .HasMaxLength(1000);

            builder.Property(center => center.DeliverySupported)
                .IsRequired();

            builder.Property(center => center.Latitude)
                .HasPrecision(9, 6);

            builder.Property(center => center.Longitude)
                .HasPrecision(9, 6);

            builder.Property(center => center.RejectionReason)
                .HasMaxLength(500);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(center => center.VerifiedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(sc => sc.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(sc => sc.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(sc => sc.Rating)
                .IsRequired()
                .HasColumnType("decimal(3,2)");

            builder.HasOne(sc => sc.Owner)
                .WithOne(u => u.ServiceCenter)
                .HasForeignKey<ServiceCenter>(sc => sc.OwnerUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(sc => sc.VerificationStatus)
                .HasConversion<string>()
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(sc => sc.IsBanned)
                .IsRequired();

            builder.Property(sc => sc.IsDeleted)
                     .IsRequired();

            builder.Property(sc => sc.ConsecutiveDelayCount)
                .IsRequired();

            builder.Property(sc => sc.CreatedAt)
                .IsRequired();

            builder.Property(sc => sc.BanEscalationCount) 
                .IsRequired();

            builder.HasIndex(sc => sc.Phone).IsUnique();
            builder.HasIndex(sc => sc.OwnerUserId).IsUnique();

            builder.HasQueryFilter(sc => !sc.IsDeleted);


        }
    
    }
}
