using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarFix.Infrastructure.Persistence.Configurations
{
    public class PasswordResetTokenConfiguration
        : IEntityTypeConfiguration<PasswordResetToken>
    {
        public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
        {
            builder.ToTable("PasswordResetTokens");

            builder.HasKey(token => token.Id);

            builder.Property(token => token.TokenHash)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(token => token.CreatedAt)
                .IsRequired();

            builder.Property(token => token.ExpiresAt)
                .IsRequired();

            builder.HasIndex(token => token.TokenHash)
                .IsUnique();

            builder.HasIndex(token => token.UserId);

            builder.HasOne(token => token.User)
                .WithMany()
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}