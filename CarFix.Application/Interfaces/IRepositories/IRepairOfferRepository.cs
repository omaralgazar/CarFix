using CarFix.Domain.Entities;

namespace CarFix.Application.Interfaces.IRepositories
{
    public interface IRepairOfferRepository
    {
        Task<RepairOffer?> GetByRequestAndCenterAsync(
            Guid repairRequestId,
            Guid centerId);

        Task<RepairOffer?> GetByIdAsync(Guid offerId);

        Task<IEnumerable<RepairOffer>> GetByCenterAsync(Guid centerId);

        Task<IEnumerable<RepairOffer>> GetByRequestIdAsync(
            Guid repairRequestId);

        Task AddAsync(RepairOffer repairOffer);

        Task SaveChangesAsync();
    }
}