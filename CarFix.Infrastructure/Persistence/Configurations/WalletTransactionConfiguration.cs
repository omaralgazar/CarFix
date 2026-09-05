using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarFix.Infrastructure.Persistence.Configurations
{
    public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.ToTable("WalletTransactions");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(t => t.Amount).HasPrecision(18, 2).IsRequired();

        }
    }
}