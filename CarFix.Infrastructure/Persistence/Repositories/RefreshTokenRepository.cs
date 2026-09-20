using System;
using System.Collections.Generic;
using System.Text;
using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CarFix.Application.Interfaces.IRepositories;

namespace CarFix.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly CarFixDbContext _context;

        public RefreshTokenRepository(CarFixDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshToken refreshToken) => await _context.RefreshTokens.AddAsync(refreshToken);
        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash) =>  await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
        public async Task RevokeAsync(RefreshToken refreshToken)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Id == refreshToken.Id);
            if (token != null)
            {
                token.IsRevoked = true;
            }
        }
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
