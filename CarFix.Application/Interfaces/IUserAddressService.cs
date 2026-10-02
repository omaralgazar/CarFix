using CarFix.Application.DTOs.Customer.Address;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces
{
    public interface IUserAddressService
    {
        Task<UserAddressResponseDto> CreateAddressAsync(Guid userId,CreateUserAddressDto addressDto);

        Task<UserAddressResponseDto> GetAddressByIdAsync(Guid addressId,Guid userId);

        Task<List<UserAddressResponseDto>> GetAllAddressesByUserIdAsync(Guid userId);

        Task<UserAddressResponseDto> UpdateAddressAsync(Guid addressId,Guid userId,UpdateUserAddressDto addressDto);
        Task DeleteAddressAsync(Guid addressId,Guid userId);
        Task SetDefaultAddressAsync(Guid addressId,Guid userId);
    }
}
