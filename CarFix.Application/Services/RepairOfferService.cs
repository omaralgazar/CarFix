using CarFix.Application.DTOs.ServiceCenter.RepairOfferDto;
using CarFix.Application.Exceptions;
using CarFix.Application.Interfaces;
using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using CarFix.Domain.Enums;
namespace CarFix.Application.Services
{
    
    public class RepairOfferService : IRepairOfferService
    {
        private readonly IRepairOfferRepository _repairOfferRepository;
        private readonly IRepairRequestRepository _repairRequestRepository;
        private readonly IServiceCenterRepository _serviceCenterRepository;

        public RepairOfferService(
            IRepairOfferRepository repairOfferRepository,
            IRepairRequestRepository repairRequestRepository,
            IServiceCenterRepository serviceCenterRepository)
        {
            _repairOfferRepository = repairOfferRepository;
            _repairRequestRepository = repairRequestRepository;
            _serviceCenterRepository = serviceCenterRepository;
        }

        public async Task<OfferResponseDto> CreateOfferAsync(Guid centerOwnerId, CreateOfferDto dto)
        {
            var serviceCenter = await _serviceCenterRepository.GetByOwnerIdAsync(centerOwnerId);
            if (serviceCenter == null)
                throw new NotFoundException("Service center not found.");
            if (serviceCenter.OwnerUserId != centerOwnerId)
                throw new BadRequestException("You are not the owner of this service center.");
            if (serviceCenter.IsBanned ||
                serviceCenter.VerificationStatus != VerificationStatus.Approved)
            {
                throw new BadRequestException(
                    "Service center is not allowed to submit offers.");
            }
            var repairRequest = await _repairRequestRepository.GetByIdAsync(dto.RepairRequestId);
            if (repairRequest == null)
                throw new NotFoundException("Repair request not found.");

            if (repairRequest.Status != RepairRequestStatus.OpenForBidding)
                throw new BadRequestException("This request is no longer accepting offers.");
            if (dto.Cost <= 0)
                throw new BadRequestException("Offer cost must be greater than zero.");

            if (dto.DurationInHours <= 0)
                throw new BadRequestException(
                    "Estimated duration must be greater than zero.");

            decimal deliveryFee = 0;
            decimal? estimatedDistanceKm = null;

            if (repairRequest.FulfillmentMethod ==
                FulfillmentMethod.CenterPickupAndReturn)
            {
                if (!serviceCenter.DeliverySupported)
                    throw new BadRequestException(
                        "This service center does not support delivery.");

                if (!dto.DeliveryFee.HasValue ||
                    !dto.EstimatedDistanceKm.HasValue)
                {
                    throw new BadRequestException(
                        "Delivery fee and estimated distance are required.");
                }

                if (dto.DeliveryFee.Value < 0 ||
                    dto.EstimatedDistanceKm.Value < 0)
                {
                    throw new BadRequestException(
                        "Delivery fee and estimated distance cannot be negative.");
                }

                deliveryFee = dto.DeliveryFee.Value;
                estimatedDistanceKm = dto.EstimatedDistanceKm.Value;
            }
            else if (dto.DeliveryFee.HasValue ||
                     dto.EstimatedDistanceKm.HasValue)
            {
                throw new BadRequestException(
                    "Delivery details are only allowed for delivery requests.");
            }
            if (repairRequest.Vehicle == null)
            {
                throw new NotFoundException("Vehicle not found.");
            }

            var vehicleBrand = repairRequest.Vehicle.Brand
                .Trim()
                .ToUpperInvariant();

            var canHandleRequest = serviceCenter.Capabilities.Any(capability =>
                capability.IssueCategory == repairRequest.IssueCategory &&
                (
                    capability.VehicleBrand == null ||
                    capability.VehicleBrand == vehicleBrand
                ));

            if (!canHandleRequest)
            {
                throw new BadRequestException(
                    "This service center does not support this repair request.");
            }
            var existingOffer = await _repairOfferRepository.GetByRequestAndCenterAsync(dto.RepairRequestId, serviceCenter.Id);
            if (existingOffer != null)
                throw new ConflictException("You have already submitted an offer for this request.");

            var repairOffer = new RepairOffer
            {
                Id = Guid.NewGuid(),
                RequestId = dto.RepairRequestId,
                CenterId = serviceCenter.Id,
                Cost = dto.Cost,
                DurationInHours = dto.DurationInHours,
                Status = RepairOfferStatus.Pending,
                DeliveryFee = deliveryFee,
                EstimatedDistanceKm = estimatedDistanceKm,
                CreatedAt = DateTime.UtcNow
            };

            await _repairOfferRepository.AddAsync(repairOffer);
            await _repairOfferRepository.SaveChangesAsync();

            return MapToResponseDto(repairRequest, serviceCenter, repairOffer);
        }

        public async Task<OfferResponseDto> UpdateOfferAsync(Guid centerOwnerId, Guid offerId, UpdateOfferDto dto)
        {
            var serviceCenter = await _serviceCenterRepository.GetByOwnerIdAsync(centerOwnerId);
            if (serviceCenter == null)
                throw new NotFoundException("Service center not found.");

            var repairOffer = await _repairOfferRepository.GetByIdAsync(offerId);
            if (repairOffer == null || repairOffer.CenterId != serviceCenter.Id)
                throw new NotFoundException("Repair offer not found or does not belong to the service center.");

            if (repairOffer.Status != RepairOfferStatus.Pending)
                throw new BadRequestException("Only pending offers can be updated.");
            var repairRequest = await _repairRequestRepository
    .GetByIdAsync(repairOffer.RequestId);

            if (repairRequest == null)
                throw new NotFoundException("Repair request not found.");

            if (repairRequest.Status != RepairRequestStatus.OpenForBidding)
                throw new BadRequestException(
                    "Offers can only be updated while the request is open for bidding.");

            if (serviceCenter.IsBanned ||
                serviceCenter.VerificationStatus != VerificationStatus.Approved)
            {
                throw new BadRequestException(
                    "Service center is not allowed to update offers.");
            }

            if (dto.Cost <= 0)
                throw new BadRequestException("Offer cost must be greater than zero.");

            if (dto.DurationInHours <= 0)
                throw new BadRequestException(
                    "Estimated duration must be greater than zero.");

            decimal deliveryFee = 0;
            decimal? estimatedDistanceKm = null;

            if (repairRequest.FulfillmentMethod ==
                FulfillmentMethod.CenterPickupAndReturn)
            {
                if (!serviceCenter.DeliverySupported)
                    throw new BadRequestException(
                        "This service center does not support delivery.");

                if (!dto.DeliveryFee.HasValue ||
                    !dto.EstimatedDistanceKm.HasValue)
                {
                    throw new BadRequestException(
                        "Delivery fee and estimated distance are required.");
                }

                if (dto.DeliveryFee.Value < 0 ||
                    dto.EstimatedDistanceKm.Value < 0)
                {
                    throw new BadRequestException(
                        "Delivery fee and estimated distance cannot be negative.");
                }

                deliveryFee = dto.DeliveryFee.Value;
                estimatedDistanceKm = dto.EstimatedDistanceKm.Value;
            }
            else if (dto.DeliveryFee.HasValue ||
                     dto.EstimatedDistanceKm.HasValue)
            {
                throw new BadRequestException(
                    "Delivery details are only allowed for delivery requests.");
            }

            repairOffer.Cost = dto.Cost;
            repairOffer.DurationInHours = dto.DurationInHours;
            repairOffer.DeliveryFee = deliveryFee;
            repairOffer.EstimatedDistanceKm = estimatedDistanceKm;

            await _repairOfferRepository.SaveChangesAsync();

            return MapToResponseDto(repairRequest, serviceCenter, repairOffer);
        }

        public async Task WithdrawOfferAsync(Guid centerOwnerId, Guid offerId)
        {
            var serviceCenter = await _serviceCenterRepository.GetByOwnerIdAsync(centerOwnerId);
            if (serviceCenter == null)
                throw new NotFoundException("Service center not found.");

            var repairOffer = await _repairOfferRepository.GetByIdAsync(offerId);
            if (repairOffer == null || repairOffer.CenterId != serviceCenter.Id)
                throw new NotFoundException("Repair offer not found or does not belong to the service center.");

            if (repairOffer.Status != RepairOfferStatus.Pending)
                throw new BadRequestException("Only pending offers can be withdrawn.");

            repairOffer.Status = RepairOfferStatus.Withdrawn;
            await _repairOfferRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<OfferResponseDto>> GetMyOffersAsync(Guid centerOwnerId)
        {
            var serviceCenter = await _serviceCenterRepository.GetByOwnerIdAsync(centerOwnerId);
            if (serviceCenter == null)
                throw new NotFoundException("Service center not found.");

            var repairOffers = await _repairOfferRepository.GetByCenterAsync(serviceCenter.Id);
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

        private static OfferResponseDto MapToResponseDto(RepairRequest repairRequest, ServiceCenter serviceCenter, RepairOffer repairOffer)
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
                DeliveryFee = repairOffer.DeliveryFee,
                EstimatedDistanceKm = repairOffer.EstimatedDistanceKm,
                TotalCost = repairOffer.Cost + repairOffer.DeliveryFee,
                CreatedAt = repairOffer.CreatedAt
            };
        }
    }
}