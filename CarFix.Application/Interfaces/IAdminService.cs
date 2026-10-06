using CarFix.Application.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<AdminServiceCenterResponseDto>> GetAllPendingServiceCentersAsync();
        Task<AdminServiceCenterResponseDto> GetServiceCenterDetailsAsync(Guid centerId);
        Task ApproveServiceCenterAsync(Guid adminid,Guid centerId);
        Task RejectServiceCenterAsync( Guid adminid,Guid centerId ,RejectServiceCenterDto dto);

    }
}
