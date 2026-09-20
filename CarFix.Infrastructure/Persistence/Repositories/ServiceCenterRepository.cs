using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using CarFix.Domain.Enums;
using CarFix.Infrastructure.Persistence;
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
                .Include(c => c.Specialties)
                .FirstOrDefaultAsync(c => c.Id == centerId && !c.IsDeleted);
        }

        public async Task<ServiceCenter?> GetByOwnerIdAsync(Guid ownerId)
        {
            return await _context.ServiceCenters
                .Include(c => c.Specialties)
                .FirstOrDefaultAsync(c => c.OwnerUserId == ownerId && !c.IsDeleted);
        }

        public async Task<IEnumerable<ServiceCenter>> GetAllPendingAsync()
        {
            return await _context.ServiceCenters
                .Include(c => c.Specialties)
                .Where(c => c.VerificationStatus == VerificationStatus.Pending && !c.IsDeleted)
                .ToListAsync();
        }

        public async Task AddAsync(ServiceCenter center)
        {
            await _context.ServiceCenters.AddAsync(center);
        }

        public void Update(ServiceCenter center)
        {
            _context.ServiceCenters.Update(center);
        }

        public async Task<CenterSpecialty?> GetSpecialtyByIdAsync(Guid specialtyId)
        {
            return await _context.CenterSpecialties
                .FirstOrDefaultAsync(s => s.Id == specialtyId);
        }

        public async Task<IEnumerable<CenterSpecialty>> GetSpecialtiesByCenterIdAsync(Guid centerId)
        {
            return await _context.CenterSpecialties
                .Where(s => s.ServiceCenterId == centerId)
                .ToListAsync();
        }

        public async Task AddSpecialtyAsync(CenterSpecialty specialty)
        {
            await _context.CenterSpecialties.AddAsync(specialty);
        }

        public void RemoveSpecialty(CenterSpecialty specialty)
        {
            _context.CenterSpecialties.Remove(specialty);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}