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

        Task<CenterCapability?> GetCapabilityByIdAsync(Guid capabilityId);

        Task<IEnumerable<CenterCapability>> GetCapabilitiesByCenterIdAsync(
            Guid centerId);

        Task AddCapabilityAsync(CenterCapability capability);

        void RemoveCapability(CenterCapability capability);

        Task SaveChangesAsync();
    }
}