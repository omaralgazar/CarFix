using CarFix.Application.DTOs.ServiceCenter.RepairOfferDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces
{
    public interface IRepairOfferService
    {
        Task<OfferResponseDto> CreateOfferAsync(Guid centerOwnerId, CreateOfferDto dto);
        Task<OfferResponseDto> UpdateOfferAsync(Guid centerOwnerId, Guid offerId, UpdateOfferDto dto);
        Task WithdrawOfferAsync(Guid centerOwnerId, Guid offerId);
        Task<IEnumerable<OfferResponseDto>> GetMyOffersAsync(Guid centerOwnerId);
        Task<IEnumerable<OfferResponseDto>> GetOffersForRequestAsync(Guid customerUserId, Guid repairRequestId);
    }
}
