using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarFix.Infrastructure.Persistence.Configurations
{
    public class TokenTransactionConfiguration : IEntityTypeConfiguration<TokenTransaction>
    {
        public void Configure(EntityTypeBuilder<TokenTransaction> builder)
        {
            builder.ToTable("TokenTransactions");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(t => t.TokensAmount).IsRequired();

            builder.HasOne(t => t.Customer)
                .WithMany(u => u.TokenTransactions)
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}