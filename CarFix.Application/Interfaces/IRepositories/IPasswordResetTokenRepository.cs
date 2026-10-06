using CarFix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces.IRepositories
{
    public interface IPasswordResetTokenRepository
    {
        Task AddAsync(PasswordResetToken passwordResetToken);

        Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash);

        Task RevokeActiveTokensForUserAsync(Guid userId);

        Task SaveChangesAsync();
    }
}
