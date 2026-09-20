using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarFix.Infrastructure.Persistence.Configurations
{
    public class DisputeConfiguration : IEntityTypeConfiguration<Dispute>
    {
        public void Configure(EntityTypeBuilder<Dispute> builder)
        {
            builder.ToTable("Disputes");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Reason).IsRequired().HasMaxLength(1000);
            builder.Property(d => d.Resolution).HasMaxLength(1000);
            builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(30).IsRequired();

            builder.HasOne(d => d.RaisedBy)
                .WithMany()
                .HasForeignKey(d => d.RaisedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Staff)
                .WithMany()
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}