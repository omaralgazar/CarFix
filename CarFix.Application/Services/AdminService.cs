using CarFix.Application.DTOs.Admin;
using CarFix.Application.DTOs.ServiceCenter;
using CarFix.Application.Interfaces;
using CarFix.Application.Interfaces.IRepositories;
using CarFix.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using CarFix.Domain.Entities;
using CarFix.Domain.Enums;

namespace CarFix.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IServiceCenterRepository _serviceCenterRepository;
        public AdminService(IServiceCenterRepository serviceCenterRepository)
        {
            _serviceCenterRepository = serviceCenterRepository;
        }
        public async Task<IEnumerable<AdminServiceCenterResponseDto>> GetAllPendingServiceCentersAsync()
        {
            var pendingCenters = await _serviceCenterRepository.GetAllPendingAsync();
            var responseDtos = new List<AdminServiceCenterResponseDto>();
            foreach (var center in pendingCenters)
            {
                responseDtos.Add(MapToResponseDto(center));
            }
            return responseDtos;
        }

        public async Task<AdminServiceCenterResponseDto> GetServiceCenterDetailsAsync(Guid centerId)
        {
            var center = await _serviceCenterRepository.GetByIdForAdminAsync(centerId);
            if (center == null)
            {
                throw new NotFoundException("Service center not found.");
            }
            
            return MapToResponseDto(center);
        }

        public async Task ApproveServiceCenterAsync(Guid adminid,Guid centerId)
        {
            var center = await _serviceCenterRepository.GetByIdForAdminAsync(centerId);
            if (center == null)
            {
                throw new NotFoundException("Service center not found.");
            }
            if (center.VerificationStatus != VerificationStatus.Pending )
            {
                throw new BadRequestException("Only pending service centers can be reviewed.");
            }

            if (center.Capabilities == null || center.Capabilities.Count == 0)
            {
                throw new BadRequestException("Service center must have at least one capability to be approved.");
            }
            center.VerificationStatus = VerificationStatus.Approved;
            center.VerificationReviewedAt = DateTime.UtcNow;
            center.VerifiedByAdminId = adminid;
            center.RejectionReason = null;
            _serviceCenterRepository.Update(center);
            await _serviceCenterRepository.SaveChangesAsync();
        }


        public async Task RejectServiceCenterAsync( Guid adminid,Guid centerId ,RejectServiceCenterDto dto)
        {
            var center = await _serviceCenterRepository.GetByIdForAdminAsync(centerId);
            if (center == null)
            {
                throw new NotFoundException("Service center not found.");
            }
            
            if (string.IsNullOrWhiteSpace(dto.Reason))
                throw new BadRequestException("Rejection reason is required.");

           
            center.VerificationStatus = VerificationStatus.Rejected;
            center.VerificationReviewedAt = DateTime.UtcNow;
            center.VerifiedByAdminId = adminid;
            center.RejectionReason = dto.Reason.Trim();
            _serviceCenterRepository.Update(center);
            await _serviceCenterRepository.SaveChangesAsync();
        }

        private static AdminServiceCenterResponseDto MapToResponseDto(ServiceCenter center)
        {
            var dto = new AdminServiceCenterResponseDto
            {
                CenterId = center.Id,
                CenterName = center.Name,
                CenterAddress = center.Address,
                CenterPhone = center.Phone,
                OwnerUserId = center.OwnerUserId,
                OwnerName = center.Owner.Name,
                OwnerEmail = center.Owner.Email,
                OwnerPhone = center.Owner.Phone,
                CreatedAt = center.CreatedAt,
                VerificationStatus = center.VerificationStatus.ToString(),
                Capabilities = new List<CenterCapabilityResponseDto>()
            };
            foreach (var capability in center.Capabilities)
            {
                dto.Capabilities.Add(new CenterCapabilityResponseDto
                {
                    Id = capability.Id,
                    IssueCategory = capability.IssueCategory,
                    VehicleBrand = capability.VehicleBrand
                });
            }
            return dto;
        }
    }
}
