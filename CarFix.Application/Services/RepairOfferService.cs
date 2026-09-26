using CarFix.Application.DTOs.Customer.RepairRequest;
using CarFix.Application.DTOs.ServiceCenter.RepairOfferDto;
using CarFix.Application.Exceptions;
using CarFix.Application.Interfaces;
using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using CarFix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Services
{
    public class RepairOfferService : IRepairOfferService
    {
        private readonly IRepairOfferRepository _repairOfferRepository;
        private readonly IRepairRequestRepository _repairRequestRepository;
        private readonly IServiceCenterRepository _serviceCenterRepository;
        public RepairOfferService(IRepairOfferRepository repairOfferRepository, IRepairRequestRepository repairRequestRepository, IServiceCenterRepository serviceCenterRepository)
        {
            _repairOfferRepository = repairOfferRepository;
            _repairRequestRepository = repairRequestRepository;
            _serviceCenterRepository = serviceCenterRepository;
        }

        public async Task<OfferResponseDto> CreateOfferAsync(Guid serviceCenterId, CreateOfferDto dto)
        {
            var repairRequest = await _repairRequestRepository.GetByIdAsync(dto.RepairRequestId);
            if (repairRequest == null)
                throw new NotFoundException("Repair request not found.");

            var serviceCenter = await _serviceCenterRepository.GetByIdAsync(serviceCenterId);
            if (serviceCenter == null)
                throw new NotFoundException("Service center not found.");

            var existingOffer = await _repairOfferRepository.GetByRequestAndCenterAsync(dto.RepairRequestId, serviceCenterId);
            if (existingOffer != null)
                throw new ConflictException("You have already submitted an offer for this request.");
            if (repairRequest.Status != RepairRequestStatus.OpenForBidding)
                throw new BadRequestException("This request is no longer accepting offers.");
            var repairOffer = new RepairOffer
            {
                Id = Guid.NewGuid(),
                RequestId = dto.RepairRequestId,
                CenterId = serviceCenterId,
                Cost = dto.Cost,
                DurationInHours = dto.DurationInHours,
                CreatedAt = DateTime.UtcNow
            };
            await _repairOfferRepository.AddAsync(repairOffer);
            await _repairOfferRepository.SaveChangesAsync();
            return MapToResponseDto(repairRequest, serviceCenter, repairOffer);
        }

        public async Task<OfferResponseDto> UpdateOfferAsync(Guid serviceCenterId, Guid offerId, UpdateOfferDto dto)
        {
            var repairOffer = await _repairOfferRepository.GetByIdAsync(offerId);
            if (repairOffer == null || repairOffer.CenterId != serviceCenterId)
                throw new NotFoundException("Repair offer not found or does not belong to the service center.");
            if(repairOffer.Status != RepairOfferStatus.Pending)
                throw new BadRequestException("Only pending offers can be updated.");
            repairOffer.Cost = dto.Cost;
            repairOffer.DurationInHours = dto.DurationInHours;
            await _repairOfferRepository.SaveChangesAsync();
            var repairRequest = await _repairRequestRepository.GetByIdAsync(repairOffer.RequestId);
            var serviceCenter = await _serviceCenterRepository.GetByIdAsync(serviceCenterId);
            return MapToResponseDto(repairRequest, serviceCenter, repairOffer);
        }

        public async Task WithdrawOfferAsync(Guid serviceCenterId, Guid offerId)
        {
            var repairOffer = await _repairOfferRepository.GetByIdAsync(offerId);
            if (repairOffer == null || repairOffer.CenterId != serviceCenterId)
                throw new NotFoundException("Repair offer not found or does not belong to the service center.");
            if (repairOffer.Status != RepairOfferStatus.Pending)
                throw new BadRequestException("Only pending offers can be withdrawn.");
            repairOffer.Status = Domain.Enums.RepairOfferStatus.Withdrawn;
            await _repairOfferRepository.SaveChangesAsync();
        }
        public async Task<IEnumerable<OfferResponseDto>> GetMyOffersAsync(Guid serviceCenterId)
        {
            var serviceCenter = await _serviceCenterRepository.GetByIdAsync(serviceCenterId);
            if (serviceCenter == null)
                throw new NotFoundException("Service center not found.");

            var repairOffers = await _repairOfferRepository.GetByCenterAsync(serviceCenterId);
            var offerDtos = new List<OfferResponseDto>();

            foreach (var offer in repairOffers)
            {
                var repairRequest = await _repairRequestRepository.GetByIdAsync(offer.RequestId);
                offerDtos.Add(MapToResponseDto(repairRequest, serviceCenter, offer));
            }
            return offerDtos;
        }
        public async Task<IEnumerable<OfferResponseDto>> GetOffersForRequestAsync(Guid customerUserId, Guid repairRequestId)
        {
            var repairRequest = await _repairRequestRepository.GetByIdAsync(repairRequestId);
            if (repairRequest == null || repairRequest.CustomerId != customerUserId)
                throw new NotFoundException("Repair request not found or does not belong to the customer.");

            var repairOffers = await _repairOfferRepository.GetByRequestIdAsync(repairRequestId);

            return repairOffers.Select(offer => MapToResponseDto(repairRequest, offer.Center, offer)).ToList();
        }
        private static OfferResponseDto MapToResponseDto(RepairRequest repairRequest, ServiceCenter serviceCenter , RepairOffer repairOffer)
        {
            return new OfferResponseDto
            {
                Id = repairOffer.Id,
                RepairRequestId = repairRequest.Id,
                CenterId = serviceCenter.Id,
                CenterName = serviceCenter.Name,
                CenterRating = serviceCenter.Rating,
                Cost = repairOffer.Cost,
                DurationInHours = repairOffer.DurationInHours,
                GracePeriodHours = repairOffer.GracePeriodHours,
                Status = repairOffer.Status.ToString(),
                CreatedAt = repairOffer.CreatedAt
            };
        }
    }
}
