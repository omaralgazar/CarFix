using CarFix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces.IRepositories
{
    public interface IUserAddressRepository
    {
        Task<UserAddress?> GetByIdForUserAsync(Guid addressId,Guid userId);
        Task<List<UserAddress>> GetAllByUserIdAsync(Guid userId);
        Task<bool> HasAnyAddressAsync(Guid userId);
        Task AddAsync(UserAddress address);
        Task SaveChangesAsync();
    }
}
