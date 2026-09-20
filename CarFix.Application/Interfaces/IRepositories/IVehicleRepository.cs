using CarFix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces.IRepositories
{
    public interface IVehicleRepository
    {
        Task<bool> HasAnyVehicleAsync(Guid customerId);
        Task AddAsync(Vehicle vehicle);
        Task<Vehicle?> GetByIdAsync(Guid vehicleId);
        Task DeleteAsync(Guid vehicleId);
        Task<List<Vehicle>> GetAllByCustomerAsync(Guid customerId);
       
        Task<Vehicle?> GetFirstRemainingVehicleAsync(Guid customerId, Guid excludeVehicleId);

        Task SaveChangesAsync();
    }
}
