using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Infrastructure.Persistence.Repositories
{
    public class UserAddressRepository : IUserAddressRepository
    {
        private readonly CarFixDbContext _context;

        public UserAddressRepository(CarFixDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserAddress address)
        {
            await _context.UserAddresses.AddAsync(address);
        }

        public Task<List<UserAddress>> GetAllByUserIdAsync(Guid userId)
        {
            return _context.UserAddresses
                .Where(address =>
                    address.UserId == userId &&
                    !address.IsDeleted)
                .OrderByDescending(address => address.IsDefault)
                .ThenByDescending(address => address.CreatedAt)
                .ToListAsync();
        }

        public Task<UserAddress?> GetByIdForUserAsync(
            Guid addressId,
            Guid userId)
        {
            return _context.UserAddresses
                .FirstOrDefaultAsync(address =>
                    address.Id == addressId &&
                    address.UserId == userId &&
                    !address.IsDeleted);
        }

        public Task<bool> HasAnyAddressAsync(Guid userId)
        {
            return _context.UserAddresses
                .AnyAsync(address =>
                    address.UserId == userId &&
                    !address.IsDeleted);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}