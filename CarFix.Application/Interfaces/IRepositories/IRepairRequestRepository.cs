using CarFix.Domain.Entities;
using CarFix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces.IRepositories
{
    public interface IRepairRequestRepository
    {
        Task<RepairRequest?> GetByIdAsync(Guid id);
        Task<IEnumerable<RepairRequest>> GetByCustomerIdAsync(Guid customerUserId);
        Task AddAsync(RepairRequest repairRequest);
        Task<IEnumerable<ServiceCenter>> GetMatchingServiceCentersAsync(SpecialtyType type, string value);
        Task SaveChangesAsync();
    }
}
