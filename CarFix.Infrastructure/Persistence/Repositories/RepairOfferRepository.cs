using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using CarFix.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Infrastructure.Persistence.Repositories
{
    public class RepairOfferRepository : IRepairOfferRepository
    {
        private readonly CarFixDbContext _context;
        public RepairOfferRepository(CarFixDbContext context)
        {
            _context = context;
        }

        public async Task<RepairOffer?> GetByRequestAndCenterAsync(Guid repairRequestId, Guid centerId)
        {
            return await _context.RepairOffers
                .Include(ro => ro.Request)
                .FirstOrDefaultAsync(ro => ro.RequestId == repairRequestId && ro.CenterId == centerId);
        }

        public async Task<RepairOffer?> GetByIdAsync(Guid offerId)
        {
            return await _context.RepairOffers
                .Include(offer => offer.Request)
                .Include(offer => offer.Center)
                .FirstOrDefaultAsync(offer => offer.Id == offerId);
        }

        public async Task<IEnumerable<RepairOffer>> GetByCenterAsync(Guid centerId)
        {
            return await _context.RepairOffers
                .Include(offer => offer.Center)
                .Where(offer => offer.CenterId == centerId)
                .OrderByDescending(offer => offer.CreatedAt)
                .ToListAsync();
        }
        public async Task<IEnumerable<RepairOffer>> GetByRequestIdAsync(
             Guid repairRequestId)
        {
            return await _context.RepairOffers
                .Include(offer => offer.Request)
                .Include(offer => offer.Center)
                .Where(offer =>
                    offer.RequestId == repairRequestId &&
                    offer.Status != RepairOfferStatus.Withdrawn)
                .OrderByDescending(offer => offer.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(RepairOffer repairOffer)
        {
            await _context.RepairOffers.AddAsync(repairOffer);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }

           
    }
