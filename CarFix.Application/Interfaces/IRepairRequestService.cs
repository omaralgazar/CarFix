using CarFix.Application.DTOs.Customer.RepairRequest;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces
{
    public interface IRepairRequestService
    {
        Task<RepairRequestResponseDto> CreateRequestAsync(Guid customerUserId,CreateRepairRequestDto dto);

        Task<List<RepairRequestResponseDto>> GetRepairRequestsByCustomerAsync(Guid customerId);
        Task CancelRequestAsync(Guid customerUserId, Guid requestId);
        Task<RepairRequestResponseDto> GetRequestByIdAsync(Guid customerUserId,Guid requestId);
    }
}
