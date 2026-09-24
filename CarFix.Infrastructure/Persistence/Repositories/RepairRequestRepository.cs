using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using CarFix.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Infrastructure.Persistence.Repositories
{
    public class RepairRequestRepository :IRepairRequestRepository
    {
        private readonly CarFixDbContext _context;
        public RepairRequestRepository(CarFixDbContext context)
        {
            _context = context;
        }
        public async Task<RepairRequest?> GetByIdAsync(Guid id)
        {
            return await _context.RepairRequests
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(rr => rr.Id == id );
        }

        public async Task<IEnumerable<RepairRequest>> GetByCustomerIdAsync(Guid id)
        {
            return await _context.RepairRequests
                .Include(r => r.Vehicle)
                .Where(r => r.CustomerId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(RepairRequest repairRequest)
        {
            await _context.RepairRequests.AddAsync(repairRequest);
        }

        public async Task<List<ServiceCenter>> GetMatchingServiceCentersAsync(
                 string issueCategory,
                 string vehicleBrand)
        {
            return await _context.ServiceCenters
                .Where(center =>
                    !center.IsDeleted &&
                    !center.IsBanned &&
                    center.VerificationStatus == VerificationStatus.Approved &&
                    center.Capabilities.Any(capability =>
                        capability.IssueCategory == issueCategory &&
                        (
                            capability.VehicleBrand == null ||
                            capability.VehicleBrand == vehicleBrand
                        )))
                .ToListAsync();
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
