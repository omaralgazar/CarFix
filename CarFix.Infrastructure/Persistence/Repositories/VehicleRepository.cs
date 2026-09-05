using CarFix.Application.Interfaces;
using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Infrastructure.Persistence.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly CarFixDbContext _context;

        public VehicleRepository(CarFixDbContext context)
        {
            _context = context;
        }

        public Task<bool> HasAnyVehicleAsync(Guid customerId) =>
            _context.Vehicles.AnyAsync(v => v.CustomerId == customerId && !v.IsDeleted);

        public async Task AddAsync(Vehicle vehicle) =>
            await _context.Vehicles.AddAsync(vehicle);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();

        public Task<Vehicle> GetByIdAsync(Guid vehicleId) =>
            _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId);

        public async Task DeleteAsync(Guid vehicleId)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId);
            if (vehicle != null)
            {
                vehicle.IsDeleted = true;
            }
        }
        public Task<List<Vehicle>> GetAllByCustomerAsync(Guid customerId)
        {
            return _context.Vehicles
                .Where(v => v.CustomerId == customerId && !v.IsDeleted)
                .ToListAsync();
        }

        public Task<Vehicle> GetFirstRemainingVehicleAsync(Guid customerId, Guid excludeVehicleId)
        {
            return _context.Vehicles.FirstOrDefaultAsync(v => v.CustomerId == customerId
                                                      && !v.IsDeleted
                                                      && v.Id != excludeVehicleId);
        }
    }
}