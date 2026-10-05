using CarFix.Application.DTOs.RepairOrder;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces
{
    public interface IRepairOrderService
    {
        Task<OrderTrackingResponseDto> AcceptOfferAsync(Guid customerId , Guid repairrequestId ,Guid offerId);
        Task<OtpResponseDto> GenerateCheckInOtpAsync(Guid customerId, Guid orderId);
        Task<OrderTrackingResponseDto> ConfirmCheckInAsync(Guid centerUserId, Guid orderId,ConfirmCheckInDto dto);
        Task<OtpResponseDto> GenerateCheckOutOtpAsync(Guid customerId, Guid orderId);
        Task<OrderTrackingResponseDto> ConfirmCheckOutAsync(Guid centerUserId, Guid orderId, ConfirmCheckOutDto dto);
        Task<OrderTrackingResponseDto> MarkReadyForHandoverAsync(Guid centerUserId,Guid orderId);
        Task<OrderTrackingResponseDto> GetTrackingForCustomerAsync(Guid currentUserId,Guid orderId);
        Task<OrderTrackingResponseDto> GetTrackingForServiceCenterAsync(Guid currentUserId, Guid orderId);

    }
}

