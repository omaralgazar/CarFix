using CarFix.Application.DTOs.Customer.Address;
using CarFix.Application.Exceptions;
using CarFix.Application.Interfaces;
using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Services
{
    public class UserAddressService : IUserAddressService
    {
        private readonly IUserAddressRepository _userAddressRepository;

        public UserAddressService(IUserAddressRepository userAddressRepository)
        {
            _userAddressRepository = userAddressRepository;
        }

        public async Task<UserAddressResponseDto> CreateAddressAsync(Guid userId, CreateUserAddressDto addressDto)
        {
            if (string.IsNullOrWhiteSpace(addressDto.Label) ||
                string.IsNullOrWhiteSpace(addressDto.ContactName) ||
                string.IsNullOrWhiteSpace(addressDto.ContactPhone) ||
                string.IsNullOrWhiteSpace(addressDto.AddressLine) ||
                string.IsNullOrWhiteSpace(addressDto.City) ||
                string.IsNullOrWhiteSpace(addressDto.Area))
            {
                throw new BadRequestException(
                    "All address fields are required.");
            }

            var isFirstAddress = !await _userAddressRepository
                .HasAnyAddressAsync(userId);
            var newAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Label = addressDto.Label.Trim(),
                ContactName = addressDto.ContactName.Trim(),
                ContactPhone = addressDto.ContactPhone.Trim(),
                AddressLine = addressDto.AddressLine.Trim(),
                City = addressDto.City.Trim(),
                Area = addressDto.Area.Trim(),
                Latitude = addressDto.Latitude,
                Longitude = addressDto.Longitude,
                IsDefault = isFirstAddress,
                CreatedAt = DateTime.UtcNow
            };
            await _userAddressRepository.AddAsync(newAddress);
            await _userAddressRepository.SaveChangesAsync();
            return MapToProfileDto(newAddress);
        }


        public async Task<UserAddressResponseDto> GetAddressByIdAsync(Guid addressId, Guid userId)
        {
            var address = await _userAddressRepository
                .GetByIdForUserAsync(addressId, userId);
            if (address == null)
            {
                throw new NotFoundException(
                    $"Address with ID {addressId} not found for the user.");
            }
            return MapToProfileDto(address);
        }

        public async Task<List<UserAddressResponseDto>> GetAllAddressesByUserIdAsync(Guid userId)
        {
            var addresses = await _userAddressRepository
                .GetAllByUserIdAsync(userId);
            return addresses.Select(MapToProfileDto).ToList();
        }

        public async Task<UserAddressResponseDto> UpdateAddressAsync(Guid addressId, Guid userId, UpdateUserAddressDto addressDto)
        {
            var address = await _userAddressRepository
                .GetByIdForUserAsync(addressId, userId);
            if (address == null)
            {
                throw new NotFoundException(
                    $"Address with ID {addressId} not found for the user.");
            }

            address.Label = addressDto.Label.Trim();
            address.ContactName = addressDto.ContactName.Trim();
            address.ContactPhone = addressDto.ContactPhone.Trim();
            address.AddressLine = addressDto.AddressLine.Trim();
            address.City = addressDto.City.Trim();
            address.Area = addressDto.Area.Trim();
            address.Latitude = addressDto.Latitude;
            address.Longitude = addressDto.Longitude;

            await _userAddressRepository.SaveChangesAsync();
            return MapToProfileDto(address);
        }

        public async Task DeleteAddressAsync(Guid addressId, Guid userId)
        {
            var address = await _userAddressRepository
                .GetByIdForUserAsync(addressId, userId);
            if (address == null)
            {
                throw new NotFoundException(
                    $"Address with ID {addressId} not found for the user.");
            }
            if (address.IsDefault)
            {
                var remainingAddresses = await _userAddressRepository
                    .GetAllByUserIdAsync(userId);

                var nextDefaultAddress = remainingAddresses
                    .FirstOrDefault(item => item.Id != addressId);

                if (nextDefaultAddress != null)
                {
                    nextDefaultAddress.IsDefault = true;
                }
            }
            address.IsDefault = false;
            address.IsDeleted = true;
            await _userAddressRepository.SaveChangesAsync();
        }

        public async Task SetDefaultAddressAsync(Guid addressId, Guid userId)
        {
            var address = await _userAddressRepository
                .GetByIdForUserAsync(addressId, userId);
            if (address == null)
            {
                throw new NotFoundException(
                    $"Address with ID {addressId} not found for the user.");
            }
            var allAddresses = await _userAddressRepository
                .GetAllByUserIdAsync(userId);
            foreach (var addr in allAddresses)
            {
                addr.IsDefault = false;
            }
            address.IsDefault = true;
            await _userAddressRepository.SaveChangesAsync();
        }

        private static UserAddressResponseDto MapToProfileDto(UserAddress address)
        {
            return new UserAddressResponseDto
            {
                Id = address.Id,
                Label = address.Label,
                ContactName = address.ContactName,
                ContactPhone = address.ContactPhone,
                AddressLine = address.AddressLine,
                City = address.City,
                Area = address.Area,
                Latitude = address.Latitude,
                Longitude = address.Longitude,
                IsDefault = address.IsDefault
            };
        }
    }
}
