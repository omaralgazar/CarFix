using CarFix.Application.DTOs.Customer.Vehicle;
using CarFix.Application.Exceptions;
using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Services
{
    public class VehicleService
    {
        private readonly IVehicleRepository _repository;
        public VehicleService(IVehicleRepository repository)
        {
            _repository = repository;
        }
        private static VehicleResponseDto MapToResponseDto(Vehicle vehicle) => new()
        {
            Id = vehicle.Id,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Year = vehicle.Year,
            LicensePlate = vehicle.LicensePlate,
            IsDefault = vehicle.IsDefault
        };

        private async Task<Vehicle> GetOwnedVehicleOrThrowAsync(Guid vehicleId, Guid customerId)
        {
            var vehicle = await _repository.GetByIdAsync(vehicleId);

            if (vehicle == null || vehicle.CustomerId != customerId)
                throw new NotFoundException("Vehicle not found");

            return vehicle;
        }

        public async Task<Vehicle> CreateVehicleAsync(Guid customerId, CreateVehicleDto dto)
        {
            var isFirstVehicle = !await _repository.HasAnyVehicleAsync(customerId);

            var vehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Brand = dto.Brand,
                Model = dto.Model,
                Year = dto.Year,
                LicensePlate = dto.LicensePlate,
                IsDefault = isFirstVehicle
            };

            await _repository.AddAsync(vehicle);
            await _repository.SaveChangesAsync();

            return vehicle;
        }

        public async Task<VehicleResponseDto> UpdateVehicleAsync(Guid vehicleId, Guid customerId, UpdateVehicleDto dto)
        {
            var vehicle = await GetOwnedVehicleOrThrowAsync(vehicleId, customerId);
            if(dto.Brand != null)
                vehicle.Brand = dto.Brand;
            if (dto.Model != null)
                vehicle.Model = dto.Model;
            if (dto.Year.HasValue)
                vehicle.Year = dto.Year.Value;
            if (dto.LicensePlate != null)
                vehicle.LicensePlate = dto.LicensePlate;

            await _repository.SaveChangesAsync();

            return MapToResponseDto(vehicle);

        }

        public async Task<VehicleResponseDto> GetVehicleByIdAsync(Guid vehicleId, Guid customerId)
        {
            var vehicle = await GetOwnedVehicleOrThrowAsync(vehicleId, customerId);
            return MapToResponseDto(vehicle);
        }

        public async Task<List<VehicleResponseDto>> ListVehiclesAsync(Guid customerId)
        {
            var vehicles = await _repository.GetAllByCustomerAsync(customerId);
            return vehicles.Select(MapToResponseDto).ToList();
        }

        public async Task SetDefaultVehicleAsync(Guid vehicleId, Guid customerId)
        {
            var vehicles = await _repository.GetAllByCustomerAsync(customerId);

            var targetVehicle = vehicles.FirstOrDefault(v => v.Id == vehicleId && !v.IsDeleted);
            if (targetVehicle == null)
                throw new NotFoundException("Vehicle not found or does not belong to this customer.");

            if (targetVehicle.IsDefault)
                return;

            foreach (var vehicle in vehicles)
            {
                vehicle.IsDefault = (vehicle.Id == vehicleId);
            }

            await _repository.SaveChangesAsync();
        }
        public async Task DeleteVehicleAsync(Guid vehicleId, Guid customerId)
        {
            var vehicle = await GetOwnedVehicleOrThrowAsync(vehicleId, customerId);
            var wasDefault = vehicle.IsDefault;
            await _repository.DeleteAsync(vehicleId);
            
        
            if (wasDefault)
            {
                var nextvehicle = await _repository.GetFirstRemainingVehicleAsync(customerId, vehicleId);
                if (nextvehicle != null) { nextvehicle.IsDefault = true; }

            }
            await _repository.SaveChangesAsync();
        }

    }

    
}
