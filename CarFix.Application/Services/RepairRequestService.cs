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

            // 2. التحقق من صحة نوع التخصص
            if (!Enum.TryParse<SpecialtyType>(dto.IssueDescription, true, out var specialtyType))
                throw new BadRequestException("Invalid specialty type.");

            // 3. إنشاء كائن الطلب
            var repairRequest = new RepairRequest
            {
                Id = Guid.NewGuid(),
                CustomerId = customerUserId,
                VehicleId = dto.VehicleId,
                IssueCategory = dto.IssueCategory,
                IssueDescription = dto.IssueDescription,
                Status = RepairRequestStatus.OpenForBidding,
                ImageUrls = dto.ImageUrls,
                CreatedAt = DateTime.UtcNow
            };

            await _repairRequestRepository.AddAsync(repairRequest);
            await _repairRequestRepository.SaveChangesAsync();

            // 4. استعلام المراكز المتوافقة مع الطلب (Query Matching)
            var matchingCenters = await _repairRequestRepository
                                        .GetMatchingServiceCentersAsync(
                                            repairRequest.IssueCategory,
                                            vehicle.Brand);

            // ملحوظة: مبدئياً الاستعلام يجهز المراكز المتوافقة (مستقبلاً يتم بث إشعارات SignalR لها)

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
            var responseDtos = new List<RepairRequestResponseDto>();
            foreach (var request in repairRequests)
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
                if (vehicle == null)
                {
                    throw new Exception("Vehicle not found.");
                }
                responseDtos.Add(MapToResponseDto(request, vehicle));
            }
            return responseDtos;
        }

        public async Task<RepairRequestResponseDto> GetRequestByIdAsync(Guid customerUserId, Guid requestId)
        {
            var request = await _repairRequestRepository.GetByIdAsync(requestId);
            if (request == null || request.CustomerId != customerUserId)
                throw new NotFoundException("Repair request not found.");

            var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
            if (vehicle == null)
                throw new Exception("Vehicle not found.");

            return MapToResponseDto(request, vehicle);
        }
        private static RepairRequestResponseDto MapToResponseDto(RepairRequest repairRequest, Vehicle vehicle)
        {
            return new RepairRequestResponseDto
            {
                Id = repairRequest.Id,
                VehicleId = repairRequest.VehicleId,
                VehicleModel = $"{vehicle.Brand} {vehicle.Model} ({vehicle.Year}){vehicle.LicensePlate}",
                IssueCategory = repairRequest.IssueCategory,
                IssueDescription = repairRequest.IssueDescription,
                ImageUrls = repairRequest.ImageUrls,
                Status = repairRequest.Status.ToString(),
                CreatedAt = repairRequest.CreatedAt
            };
        }
    }
}
