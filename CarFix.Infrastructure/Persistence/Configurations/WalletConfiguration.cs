// WalletConfiguration.cs
using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarFix.Infrastructure.Persistence.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("Wallets");
            builder.HasKey(w => w.Id);

            builder.Property(w => w.OwnerType).HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(w => w.Balance).HasPrecision(18, 2).IsRequired();
            builder.Property(w => w.HeldBalance).HasPrecision(18, 2).IsRequired();
            builder.Property(w => w.TokenBalance).IsRequired();
            builder.Property(w => w.RowVersion).IsRowVersion();


            builder.HasMany(w => w.Transactions)
                .WithOne(t => t.Wallet)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(w => new { w.OwnerId, w.OwnerType }).IsUnique();
        }
    }
}