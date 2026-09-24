using CarFix.Application.DTOs.ServiceCenter;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces
{
    public interface IServiceCenterServices
    {
        Task<CenterProfileResponseDto> GetProfileAsync(Guid ownerUserId);
        Task<CenterProfileResponseDto> UpdateProfileAsync(Guid ownerUserId, UpdateCenterProfileDto dto);
        Task<CenterProfileResponseDto> AddCapabilityAsync(Guid ownerUserId, AddCenterCapabilityDto dto);
        Task<CenterProfileResponseDto> UpdateCapabilityAsync(Guid ownerUserId, Guid capabilityId, UpdateCenterCapabilityDto dto);
        Task<CenterProfileResponseDto> RemoveCapabilityAsync(Guid ownerUserId, Guid capabilityId);
        Task<CenterProfileResponseDto> GetProfileByOwnerIdAsync(Guid ownerId);
        Task<CenterPublicProfileResponseDto> GetPublicProfileAsync(Guid id);
    }
}
