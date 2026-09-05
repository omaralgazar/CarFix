using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarFix.Infrastructure.Persistence.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");
            builder.HasKey(i => i.Id);

            builder.Property(i => i.BaseCost).HasPrecision(18, 2).IsRequired();
            builder.Property(i => i.ExtraScopeCost).HasPrecision(18, 2).IsRequired();
            builder.Property(i => i.PenaltyDeduction).HasPrecision(18, 2).IsRequired();
            builder.Property(i => i.InspectionFee).HasPrecision(18, 2).IsRequired();
            builder.Property(i => i.PlatformCommission).HasPrecision(18, 2).IsRequired();
            builder.Property(i => i.NetAmountToCenter).HasPrecision(18, 2).IsRequired();
            builder.Property(i => i.TotalPaidByCustomer).HasPrecision(18, 2).IsRequired();
            builder.Property(i => i.TokenDiscountAmount).HasPrecision(18, 2).IsRequired();

            builder.HasMany(i => i.WalletTransactions)
                .WithOne(wt => wt.Invoice)
                .HasForeignKey(wt => wt.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(i => i.TokenTransactions)
                .WithOne(tt => tt.Invoice)
                .HasForeignKey(tt => tt.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}