using CarFix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces.IRepositories
{
    public interface IRepairOrderRepository
    {
        Task<RepairOrder?> GetRepairOrderByIdAsync(Guid id);
        Task<RepairOrder?> GetByRepairRequestIdAsync(Guid repairRequestId);
        Task AddAsync(RepairOrder repairOrder);
        Task SaveChangesAsync();
    }
}
