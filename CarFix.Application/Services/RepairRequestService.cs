using CarFix.Application.DTOs.Customer.RepairRequest;
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
    public class RepairRequestService : IRepairRequestService
    {
        private readonly IRepairRequestRepository _repairRequestRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IUserAddressRepository _userAddressRepository;
        public RepairRequestService(IRepairRequestRepository repairRequestRepository, IVehicleRepository vehicleRepository, IUserAddressRepository userAddressRepository)
        {
            _repairRequestRepository = repairRequestRepository;
            _vehicleRepository = vehicleRepository;
            _userAddressRepository = userAddressRepository;
        }

        public async Task<RepairRequestResponseDto> CreateRequestAsync(Guid customerUserId, CreateRepairRequestDto dto)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId);
            if (vehicle == null || vehicle.VehicleOwnererId != customerUserId)
                throw new NotFoundException("Vehicle not found or does not belong to the user.");

            if (string.IsNullOrWhiteSpace(dto.IssueCategory))
                throw new BadRequestException("Issue category is required.");

            if (string.IsNullOrWhiteSpace(dto.IssueDescription))
                throw new BadRequestException("Issue description is required.");
            UserAddress? pickupAddress = null;

            if (!Enum.IsDefined(dto.FulfillmentMethod))
                throw new BadRequestException("Invalid fulfillment method.");

            if (dto.FulfillmentMethod == FulfillmentMethod.CenterPickupAndReturn)
            {
                if (!dto.PickupAddressId.HasValue)
                    throw new BadRequestException(
                        "Pickup address is required when delivery is selected.");

                pickupAddress = await _userAddressRepository.GetByIdForUserAsync(
                    dto.PickupAddressId.Value,
                    customerUserId);

                if (pickupAddress == null)
                    throw new NotFoundException(
                        "Pickup address not found or does not belong to the user.");
            }
            else if (dto.PickupAddressId.HasValue)
            {
                throw new BadRequestException(
                    "Pickup address must not be sent for customer drop-off.");
            }

            var repairRequest = new RepairRequest
            {
                Id = Guid.NewGuid(),
                CustomerId = customerUserId,
                VehicleId = dto.VehicleId,
                IssueCategory = dto.IssueCategory.Trim().ToUpperInvariant(),
                IssueDescription = dto.IssueDescription,
                Status = RepairRequestStatus.OpenForBidding,
                FulfillmentMethod = dto.FulfillmentMethod,

                PickupAddressId = pickupAddress?.Id,
                PickupContactName = pickupAddress?.ContactName,
                PickupContactPhone = pickupAddress?.ContactPhone,
                PickupAddressSnapshot = pickupAddress == null? null: $"{pickupAddress.AddressLine}, {pickupAddress.Area}, {pickupAddress.City}",
                PickupLatitude = pickupAddress?.Latitude,
                PickupLongitude = pickupAddress?.Longitude,
                ImageUrls = dto.ImageUrls,
                CreatedAt = DateTime.UtcNow
            };

            await _repairRequestRepository.AddAsync(repairRequest);
            await _repairRequestRepository.SaveChangesAsync();

            var matchingCenters = await _repairRequestRepository
                                    .GetMatchingServiceCentersAsync(
                                         repairRequest.IssueCategory,
                                         vehicle.Brand.Trim().ToUpperInvariant(),
                                         repairRequest.FulfillmentMethod ==
                                             FulfillmentMethod.CenterPickupAndReturn);


            return MapToResponseDto(repairRequest, vehicle);
        }


        public async Task CancelRequestAsync( Guid customerUserId,Guid requestId)     
        {
            var repairRequest = await _repairRequestRepository
                .GetByIdAsync(requestId);

            if (repairRequest == null ||
                repairRequest.CustomerId != customerUserId)
            {
                throw new NotFoundException("Repair request not found.");
            }

            if (repairRequest.Status != RepairRequestStatus.OpenForBidding)
            {
                throw new BadRequestException(
                    "Only requests open for bidding can be cancelled.");
            }

            repairRequest.Status = RepairRequestStatus.Cancelled;

            await _repairRequestRepository.SaveChangesAsync();
        }

        public async Task<List<RepairRequestResponseDto>> GetRepairRequestsByCustomerAsync(Guid customerId)
        {
            var repairRequests = await _repairRequestRepository.GetByCustomerIdAsync(customerId);
            return repairRequests.Select(r => MapToResponseDto(r, r.Vehicle)).ToList();
        }

        public async Task<RepairRequestResponseDto> GetRequestByIdAsync(Guid customerUserId, Guid requestId)
        {
            var request = await _repairRequestRepository.GetByIdAsync(requestId);
            if (request == null || request.CustomerId != customerUserId)
                throw new NotFoundException("Repair request not found.");

            return MapToResponseDto(request, request.Vehicle);
        }
        private static RepairRequestResponseDto MapToResponseDto(RepairRequest repairRequest, Vehicle vehicle)
        {
            return new RepairRequestResponseDto
            {
                Id = repairRequest.Id,
                VehicleId = repairRequest.VehicleId,
                VehicleModel = $"{vehicle.Brand} {vehicle.Model} ({vehicle.Year}) {vehicle.LicensePlate}",
                IssueCategory = repairRequest.IssueCategory.Trim().ToUpperInvariant(),
                IssueDescription = repairRequest.IssueDescription.Trim(),
                ImageUrls = repairRequest.ImageUrls,
                Status = repairRequest.Status.ToString(),
                FulfillmentMethod = repairRequest.FulfillmentMethod.ToString(),
                CreatedAt = repairRequest.CreatedAt
            };
        }
    }
}
