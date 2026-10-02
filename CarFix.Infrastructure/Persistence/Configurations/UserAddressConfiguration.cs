using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CarFix.Infrastructure.Persistence.Configurations
{
    public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
    {
        public void Configure(EntityTypeBuilder<UserAddress> builder)
        {
            builder.ToTable("UserAddresses");

            builder.HasKey(address => address.Id);

            builder.Property(address => address.Label)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(address => address.ContactName)
                    .IsRequired()
                    .HasMaxLength(100);

            builder.Property(address => address.ContactPhone)
                    .IsRequired()
                    .HasMaxLength(20);

            builder.Property(address => address.AddressLine)
                    .IsRequired()
                    .HasMaxLength(500);

            builder.Property(address => address.City)
                    .IsRequired()
                    .HasMaxLength(100);

            builder.Property(address => address.Area)
                    .IsRequired()
                    .HasMaxLength(100);

            builder.Property(address => address.Latitude)
                    .HasPrecision(9, 6);

            builder.Property(address => address.Longitude)
                    .HasPrecision(9, 6);

            builder.HasOne(address => address.User)
                    .WithMany(user => user.Addresses)
                    .HasForeignKey(address => address.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(address => !address.IsDeleted);
        }
    }
}
