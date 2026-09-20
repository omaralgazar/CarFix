using CarFix.Domain.Entities;

namespace CarFix.Application.Interfaces.IRepositories
{
    public interface IServiceCenterRepository
    {
        Task<ServiceCenter?> GetByIdAsync(Guid id);
        Task<ServiceCenter?> GetByOwnerIdAsync(Guid ownerId);
        Task<IEnumerable<ServiceCenter>> GetAllPendingAsync();
        Task AddAsync(ServiceCenter serviceCenter);
        void Update(ServiceCenter serviceCenter);

        Task<CenterSpecialty?> GetSpecialtyByIdAsync(Guid specialtyId);
        Task<IEnumerable<CenterSpecialty>> GetSpecialtiesByCenterIdAsync(Guid centerId);
        Task AddSpecialtyAsync(CenterSpecialty specialty);
        void RemoveSpecialty(CenterSpecialty specialty);

        Task SaveChangesAsync();
    }
}