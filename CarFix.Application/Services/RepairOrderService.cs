using CarFix.Application.DTOs.RepairOrder;
using CarFix.Application.Exceptions;
using CarFix.Application.Interfaces;
using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using CarFix.Domain.Enums;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Services
{
    public class RepairOrderService : IRepairOrderService
    {
        private readonly IRepairOrderRepository _repairOrderRepository;
        private readonly IRepairOfferRepository _repairOfferRepository;
        private readonly IRepairRequestRepository _repairRequestRepository;
        private readonly IServiceCenterRepository _serviceCenterRepository;

        public RepairOrderService(
            IRepairOrderRepository repairOrderRepository,
            IRepairOfferRepository repairOfferRepository,
            IRepairRequestRepository repairRequestRepository,
            IServiceCenterRepository serviceCenterRepository)
        {
            _repairOrderRepository = repairOrderRepository;
            _repairOfferRepository = repairOfferRepository;
            _repairRequestRepository = repairRequestRepository;
            _serviceCenterRepository = serviceCenterRepository;
        }

        public async Task<OrderTrackingResponseDto> AcceptOfferAsync(Guid customerId, Guid repairrequestId, Guid offerId)
        {
            var repairRequest = await _repairRequestRepository.GetByIdAsync(repairrequestId);
            if (repairRequest == null || repairRequest.CustomerId != customerId)
                throw new NotFoundException("Repair request not found or does not belong to the customer.");
            if (repairRequest.Status != RepairRequestStatus.OpenForBidding)
                throw new BadRequestException("This request is no longer accepting offers.");
            var repairOffer = await _repairOfferRepository.GetByIdAsync(offerId);
            if (repairOffer == null || repairOffer.RequestId != repairrequestId)
                throw new NotFoundException("Repair offer not found or does not belong to the request.");
            if (repairOffer.Status != RepairOfferStatus.Pending)
                throw new BadRequestException("This offer is no longer available for acceptance.");
            var serviceCenter = await _serviceCenterRepository.GetByIdAsync(repairOffer.CenterId);
            if (serviceCenter == null ||serviceCenter.IsBanned ||serviceCenter.IsDeleted ||serviceCenter.VerificationStatus != VerificationStatus.Approved)
                throw new NotFoundException("Service center not found or is inactive.");
            var existingOrder = await _repairOrderRepository
                                .GetByRepairRequestIdAsync(repairrequestId);

            if (existingOrder != null)
            {
                throw new ConflictException(
                    "A repair order already exists for this repair request.");
            }
            var repairOrder = new RepairOrder
            {
                Id = Guid.NewGuid(),
                RepairRequestId = repairrequestId,
                AcceptedOfferId = offerId,
                Status = RepairOrderStatus.PendingCheckIn,
                OriginalDurationInHours = repairOffer.DurationInHours,
                ExtendedDurationInHours = 0,
                FulfillmentMethod = repairRequest.FulfillmentMethod,
                PickupContactName = repairRequest.PickupContactName,
                PickupContactPhone = repairRequest.PickupContactPhone,
                PickupAddressSnapshot = repairRequest.PickupAddressSnapshot,
                PickupLatitude = repairRequest.PickupLatitude,
                PickupLongitude = repairRequest.PickupLongitude,
                DeliveryFee = repairOffer.DeliveryFee,
                EstimatedDistanceKm = repairOffer.EstimatedDistanceKm,
                CreatedAt = DateTime.UtcNow
            };

            repairOffer.Status = RepairOfferStatus.Accepted;
            repairRequest.Status = RepairRequestStatus.OfferAccepted;

            await _repairOrderRepository.AddAsync(repairOrder);
            await _repairOfferRepository.RejectOtherPendingOffersAsync(repairrequestId, offerId);
            await _repairOrderRepository.SaveChangesAsync();
            return MapToResponseDto(repairOrder, repairRequest, repairOffer, serviceCenter, repairRequest.Vehicle);
        }

        public async Task<OtpResponseDto> GenerateCheckInOtpAsync(Guid customerId, Guid orderId)
        {
            var repairOrder = await _repairOrderRepository.GetRepairOrderByIdAsync(orderId);
            if (repairOrder == null ||
                repairOrder.RepairRequest.CustomerId != customerId)
            {
                throw new NotFoundException("Repair order not found.");
            }
            if (repairOrder.Status != RepairOrderStatus.PendingCheckIn)
                throw new BadRequestException("Repair order is not in a state that allows check-in.");
            
            var otpCode = RandomNumberGenerator
                           .GetInt32(0, 10000)
                           .ToString("D4");

            var expiresAt = DateTime.UtcNow.AddMinutes(10);
            repairOrder.CheckInOTP = otpCode;
            repairOrder.CheckInOTPExpiry = expiresAt;
            await _repairOrderRepository.SaveChangesAsync();

            return new OtpResponseDto
            {
                OtpCode = otpCode,
                ExpiresAt = expiresAt
            };


        }

        public async Task<OrderTrackingResponseDto> ConfirmCheckInAsync(Guid centerUserId,Guid orderId,ConfirmCheckInDto dto)
        {
            var serviceCenter = await _serviceCenterRepository
                .GetByOwnerIdAsync(centerUserId);

            if (serviceCenter == null ||
                serviceCenter.IsBanned ||
                serviceCenter.VerificationStatus != VerificationStatus.Approved)
            {
                throw new NotFoundException(
                    "Service center not found or is inactive.");
            }

            var repairOrder = await _repairOrderRepository
                .GetRepairOrderByIdAsync(orderId);

            if (repairOrder == null ||
                repairOrder.AcceptedOffer.CenterId != serviceCenter.Id)
            {
                throw new NotFoundException("Repair order not found.");
            }

            if (repairOrder.Status != RepairOrderStatus.PendingCheckIn)
            {
                throw new BadRequestException(
                    "Repair order is not in a state that allows check-in confirmation.");
            }

            if (string.IsNullOrWhiteSpace(repairOrder.CheckInOTP) ||
                repairOrder.CheckInOTPExpiry == null ||
                repairOrder.CheckInOTPExpiry < DateTime.UtcNow ||
                repairOrder.CheckInOTP != dto.OtpCode)
            {
                throw new BadRequestException("Invalid or expired OTP code.");
            }

            var checkInTime = DateTime.UtcNow;

            repairOrder.Status = RepairOrderStatus.InProgress;
            repairOrder.CheckedInAt = checkInTime;

            repairOrder.GracePeriodEndsAt = checkInTime
                .AddHours(repairOrder.OriginalDurationInHours)
                .AddHours((double)repairOrder.AcceptedOffer.GracePeriodHours);

            repairOrder.CheckInOTP = null;
            repairOrder.CheckInOTPExpiry = null;

            await _repairOrderRepository.SaveChangesAsync();

            return MapToResponseDto(
                repairOrder,
                repairOrder.RepairRequest,
                repairOrder.AcceptedOffer,
                serviceCenter,
                repairOrder.RepairRequest.Vehicle);
        }

        public async Task<OtpResponseDto> GenerateCheckOutOtpAsync(Guid customerId, Guid orderId)
        {
            var repairOrder = await _repairOrderRepository
                                .GetRepairOrderByIdAsync(orderId);

            if (repairOrder == null ||
                repairOrder.RepairRequest.CustomerId != customerId)
            {
                throw new NotFoundException("Repair order not found.");
            }

            if (repairOrder.Status != RepairOrderStatus.ReadyForHandover)
            {
                throw new BadRequestException(
                    "Repair order is not ready for pickup.");
            }
            var otpCode = RandomNumberGenerator
                           .GetInt32(0, 10000)
                           .ToString("D4");

            var expiresAt = DateTime.UtcNow.AddMinutes(10);
            repairOrder.CheckOutOTP = otpCode;
            repairOrder.CheckOutOTPExpiry = expiresAt;
            await _repairOrderRepository.SaveChangesAsync();
            return new OtpResponseDto
            {
                OtpCode = otpCode,
                ExpiresAt = expiresAt
            };
        }

        public async Task<OrderTrackingResponseDto> ConfirmCheckOutAsync(Guid centerUserId, Guid orderId, ConfirmCheckOutDto dto)
        {
            var serviceCenter = await _serviceCenterRepository
                .GetByOwnerIdAsync(centerUserId);

            if (serviceCenter == null ||
                serviceCenter.IsBanned ||
                serviceCenter.VerificationStatus != VerificationStatus.Approved)
            {
                throw new NotFoundException(
                    "Service center not found or is inactive.");
            }

            var repairOrder = await _repairOrderRepository
                .GetRepairOrderByIdAsync(orderId);

            if (repairOrder == null ||
                repairOrder.AcceptedOffer.CenterId != serviceCenter.Id)
            {
                throw new NotFoundException("Repair order not found.");
            }

            if (repairOrder.Status != RepairOrderStatus.ReadyForHandover)
            {
                throw new BadRequestException(
                    "Repair order is not in a state that allows check-out confirmation.");
            }

            if (string.IsNullOrWhiteSpace(repairOrder.CheckOutOTP) ||
                repairOrder.CheckOutOTPExpiry == null ||
                repairOrder.CheckOutOTPExpiry < DateTime.UtcNow ||
                repairOrder.CheckOutOTP != dto.OtpCode)
            {
                throw new BadRequestException("Invalid or expired OTP code.");
            }

            
            var checkOutTime = DateTime.UtcNow;

            repairOrder.Status = RepairOrderStatus.Completed;
            repairOrder.CheckedOutAt = checkOutTime;

            repairOrder.CheckOutOTP = null;
            repairOrder.CheckOutOTPExpiry = null;

            await _repairOrderRepository.SaveChangesAsync();
            return MapToResponseDto(
                repairOrder,
                repairOrder.RepairRequest,
                repairOrder.AcceptedOffer,
                serviceCenter,
                repairOrder.RepairRequest.Vehicle);
        }

        public async Task<OrderTrackingResponseDto> MarkReadyForHandoverAsync(Guid centerUserId, Guid orderId)
        {
           var serviceCenter = await _serviceCenterRepository.GetByOwnerIdAsync(centerUserId);
            if (serviceCenter == null || serviceCenter.IsBanned || serviceCenter.VerificationStatus != VerificationStatus.Approved)
                throw new NotFoundException("Service center not found or is inactive.");
            var repairOrder = await _repairOrderRepository.GetRepairOrderByIdAsync(orderId);
            if (repairOrder == null || repairOrder.AcceptedOffer.CenterId != serviceCenter.Id)
                throw new NotFoundException("Repair order not found.");
            if (repairOrder.Status != RepairOrderStatus.InProgress)
                throw new BadRequestException("Repair order is not in a state that allows marking as ready for pickup.");
            repairOrder.Status = RepairOrderStatus.ReadyForHandover;
            
            await _repairOrderRepository.SaveChangesAsync();
            return MapToResponseDto(repairOrder, repairOrder.RepairRequest, repairOrder.AcceptedOffer, serviceCenter, repairOrder.RepairRequest.Vehicle);
        }
         
        public async Task<OrderTrackingResponseDto> GetTrackingForCustomerAsync(Guid currentUserId, Guid orderId)
        {
            var repairOrder = await _repairOrderRepository.GetRepairOrderByIdAsync(orderId);
            if (repairOrder == null ||
                     repairOrder.RepairRequest.CustomerId != currentUserId )
            {
                throw new NotFoundException("Repair order not found.");
            }
            
            return MapToResponseDto(repairOrder, repairOrder.RepairRequest, repairOrder.AcceptedOffer, repairOrder.AcceptedOffer.Center, repairOrder.RepairRequest.Vehicle);

        }

        public async Task<OrderTrackingResponseDto> GetTrackingForServiceCenterAsync(Guid currentUserId, Guid orderId)
        {
            var repairOrder = await _repairOrderRepository.GetRepairOrderByIdAsync(orderId);
            if (repairOrder == null ||
                     repairOrder.AcceptedOffer.Center.OwnerUserId != currentUserId)
            {
                throw new NotFoundException("Repair order not found.");
            }

            return MapToResponseDto(repairOrder, repairOrder.RepairRequest, repairOrder.AcceptedOffer, repairOrder.AcceptedOffer.Center, repairOrder.RepairRequest.Vehicle);

        }

        private OrderTrackingResponseDto MapToResponseDto(RepairOrder repairOrder ,RepairRequest repairRequest ,RepairOffer repairOffer, ServiceCenter serviceCenter, Vehicle vehicle)
        {
            return new OrderTrackingResponseDto
            {
                OrderId = repairOrder.Id,
                RepairRequestId = repairRequest.Id,
                AcceptedOfferId = repairOffer.Id,
                ServiceCenterName = serviceCenter.Name,
                VehicleBrand = vehicle.Brand,
                VehicleModel = vehicle.Model,
                VehicleYear = vehicle.Year,
                Status = repairOrder.Status.ToString(),
                Cost = repairOffer.Cost,
                OriginalDurationInHours = repairOffer.DurationInHours,
                ExtendedDurationInHours = repairOrder.ExtendedDurationInHours,
                FulfillmentMethod = repairOrder.FulfillmentMethod.ToString(),
                DeliveryFee = repairOrder.DeliveryFee,
                EstimatedDistanceKm = repairOrder.EstimatedDistanceKm,
                TotalCost = repairOffer.Cost + repairOrder.DeliveryFee,
                PickupContactName = repairOrder.PickupContactName,
                PickupContactPhone = repairOrder.PickupContactPhone,
                PickupAddressSnapshot = repairOrder.PickupAddressSnapshot,
                PickupLatitude = repairOrder.PickupLatitude,
                PickupLongitude = repairOrder.PickupLongitude,
                CreatedAt = repairOrder.CreatedAt,
                CheckedInAt = repairOrder.CheckedInAt,
                CheckedOutAt = repairOrder.CheckedOutAt,
                GracePeriodEndsAt = repairOrder.GracePeriodEndsAt,
                
            };
        }
    }
}
