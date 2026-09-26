using CarFix.Application.DTOs.ServiceCenter.RepairOfferDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces
{
    public interface IRepairOfferService
    {
        Task<OfferResponseDto> CreateOfferAsync(Guid centerUserId, CreateOfferDto dto);
        Task<OfferResponseDto> UpdateOfferAsync(Guid centerUserId, Guid offerId, UpdateOfferDto dto);
        Task WithdrawOfferAsync(Guid centerUserId, Guid offerId);
        Task<IEnumerable<OfferResponseDto>> GetMyOffersAsync(Guid centerUserId);
        Task<IEnumerable<OfferResponseDto>> GetOffersForRequestAsync(Guid customerUserId, Guid repairRequestId);
    }
}
