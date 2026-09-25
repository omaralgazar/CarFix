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
        public RepairRequestService(IRepairRequestRepository repairRequestRepository, IVehicleRepository vehicleRepository)
        {
            _repairRequestRepository = repairRequestRepository;
            _vehicleRepository = vehicleRepository;
        }

        public async Task<RepairRequestResponseDto> CreateRequestAsync(Guid customerUserId, CreateRepairRequestDto dto)
        {
            // 1. التحقق من وجود السيارة وتبعيّتها للعميل
            var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId);
            if (vehicle == null || vehicle.VehicleOwnererId != customerUserId)
                throw new NotFoundException("Vehicle not found or does not belong to the user.");


            var repairRequest = new RepairRequest
            {
                Id = Guid.NewGuid(),
                CustomerId = customerUserId,
                VehicleId = dto.VehicleId,
                IssueCategory = dto.IssueCategory.Trim().ToUpperInvariant(),
                IssueDescription = dto.IssueDescription,
                Status = RepairRequestStatus.OpenForBidding,
                ImageUrls = dto.ImageUrls,
                CreatedAt = DateTime.UtcNow
            };

            await _repairRequestRepository.AddAsync(repairRequest);
            await _repairRequestRepository.SaveChangesAsync();

            var matchingCenters = await _repairRequestRepository
                                        .GetMatchingServiceCentersAsync(
                                            repairRequest.IssueCategory,
                                            vehicle.Brand.Trim().ToUpperInvariant());


            return MapToResponseDto(repairRequest, vehicle);
        }


        public async Task CancelRequestAsync(
                                                Guid customerUserId,
                                                Guid requestId)     
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
                IssueDescription = repairRequest.IssueDescription,
                ImageUrls = repairRequest.ImageUrls,
                Status = repairRequest.Status.ToString(),
                CreatedAt = repairRequest.CreatedAt
            };
        }
    }
}
