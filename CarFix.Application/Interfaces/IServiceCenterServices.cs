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
        Task<CenterProfileResponseDto> AddSpecialtyAsync(Guid ownerUserId, AddSpecialtyDto dto);
        Task<CenterProfileResponseDto> UpdateSpecialtyAsync(Guid ownerUserId, Guid specialtyId, UpdateCenterSpecialityDto dto);
        Task<CenterProfileResponseDto> RemoveSpecialtyAsync(Guid ownerUserId, Guid specialtyId);
        Task<CenterProfileResponseDto> GetProfileByOwnerIdAsync(Guid ownerId);
        Task<CenterPublicProfileResponseDto> GetPublicProfileAsync(Guid id);
    }
}
