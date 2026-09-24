using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using CarFix.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Infrastructure.Persistence.Repositories
{
    public class ServiceCenterRepository : IServiceCenterRepository
    {
        private readonly CarFixDbContext _context;

        public ServiceCenterRepository(CarFixDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceCenter?> GetByIdAsync(Guid centerId)
        {
            return await _context.ServiceCenters
                .Include(center => center.Capabilities)
                .FirstOrDefaultAsync(center =>
                    center.Id == centerId &&
                    !center.IsDeleted);
        }

        public async Task<ServiceCenter?> GetByOwnerIdAsync(Guid ownerId)
        {
            return await _context.ServiceCenters
                .Include(center => center.Capabilities)
                .FirstOrDefaultAsync(center =>
                    center.OwnerUserId == ownerId &&
                    !center.IsDeleted);
        }

        public async Task<IEnumerable<ServiceCenter>> GetAllPendingAsync()
        {
            return await _context.ServiceCenters
                .Include(center => center.Capabilities)
                .Where(center =>
                    center.VerificationStatus == VerificationStatus.Pending &&
                    !center.IsDeleted)
                .ToListAsync();
        }

        public async Task AddAsync(ServiceCenter serviceCenter)
        {
            await _context.ServiceCenters.AddAsync(serviceCenter);
        }

        public void Update(ServiceCenter serviceCenter)
        {
            _context.ServiceCenters.Update(serviceCenter);
        }

        public async Task<CenterCapability?> GetCapabilityByIdAsync(
            Guid capabilityId)
        {
            return await _context.CenterCapabilities
                .FirstOrDefaultAsync(capability =>
                    capability.Id == capabilityId);
        }

        public async Task<IEnumerable<CenterCapability>>
            GetCapabilitiesByCenterIdAsync(Guid centerId)
        {
            return await _context.CenterCapabilities
                .Where(capability =>
                    capability.ServiceCenterId == centerId)
                .ToListAsync();
        }

        public async Task AddCapabilityAsync(CenterCapability capability)
        {
            await _context.CenterCapabilities.AddAsync(capability);
        }

        public void RemoveCapability(CenterCapability capability)
        {
            _context.CenterCapabilities.Remove(capability);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}