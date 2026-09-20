using CarFix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces.IRepositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
