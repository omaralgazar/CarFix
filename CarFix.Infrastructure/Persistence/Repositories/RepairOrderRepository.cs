using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Infrastructure.Persistence.Repositories
{
    public class RepairOrderRepository : IRepairOrderRepository
    {
        private readonly CarFixDbContext _context;
        public RepairOrderRepository(CarFixDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(RepairOrder repairOrder)
        {
            await _context.RepairOrders.AddAsync(repairOrder);
        }
        public async Task<RepairOrder?> GetByRepairRequestIdAsync(Guid repairRequestId)
        {
            return await _context.RepairOrders
                 .Include(order => order.RepairRequest)
                     .ThenInclude(request => request.Vehicle)
                 .Include(order => order.AcceptedOffer)
                     .ThenInclude(offer => offer.Center)
                 .FirstOrDefaultAsync(order =>
                     order.RepairRequestId == repairRequestId);
        }
        public async Task<RepairOrder?> GetRepairOrderByIdAsync(Guid id)
        {
            return await _context.RepairOrders
                .Include(order => order.RepairRequest)
                    .ThenInclude(request => request.Vehicle)
                .Include(order => order.AcceptedOffer)
                    .ThenInclude(offer => offer.Center)
                .FirstOrDefaultAsync(order => order.Id == id);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
