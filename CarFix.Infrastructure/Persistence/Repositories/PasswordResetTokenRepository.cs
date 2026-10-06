using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Infrastructure.Persistence.Repositories
{
    public class PasswordResetTokenRepository
        : IPasswordResetTokenRepository
    {
        private readonly CarFixDbContext _context;

        public PasswordResetTokenRepository(CarFixDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PasswordResetToken passwordResetToken)
        {
            await _context.PasswordResetTokens.AddAsync(passwordResetToken);
        }

        public Task<PasswordResetToken?> GetByTokenHashAsync(
            string tokenHash)
        {
            return _context.PasswordResetTokens
                .FirstOrDefaultAsync(token => token.TokenHash == tokenHash);
        }

        public async Task RevokeActiveTokensForUserAsync(Guid userId)
        {
            var activeTokens = await _context.PasswordResetTokens
                .Where(token =>
                    token.UserId == userId &&
                    token.UsedAt == null &&
                    token.RevokedAt == null &&
                    token.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}