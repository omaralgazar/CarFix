using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Infrastructure.Persistence.Configurations
{
    public class ServiceCenterImageConfiguration : IEntityTypeConfiguration<ServiceCenterImages>
    {
        public void Configure(EntityTypeBuilder<ServiceCenterImages> builder)
        {
            builder.ToTable("ServiceCenterImages");

            builder.HasKey(image => image.Id);

            builder.Property(image => image.ImageUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.HasOne(image => image.ServiceCenter)
                .WithMany(center => center.Images)
                .HasForeignKey(image => image.ServiceCenterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(image => new
            {
                image.ServiceCenterId,
                image.DisplayOrder
            });
        }
        }
}
