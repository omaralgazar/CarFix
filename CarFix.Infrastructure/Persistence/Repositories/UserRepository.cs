using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using CarFix.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly CarFixDbContext _context;

        public UserRepository(CarFixDbContext context)
        {
            _context = context;
        }
        public Task<User?> GetByIdAsync(Guid id)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
        public Task<User?> GetByEmailAsync(string email)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task AddAsync(User user) => await _context.Users.AddAsync(user);
        public Task<bool> HasSuperAdminAsync()
        {
            return _context.Users.AnyAsync(user =>
                user.Role == UserRoles.SuperAdmin &&
                !user.IsDeleted);
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();


    }
}
