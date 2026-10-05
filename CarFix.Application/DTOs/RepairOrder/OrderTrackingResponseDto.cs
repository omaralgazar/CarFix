using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.DTOs.RepairOrder
{
    public class OrderTrackingResponseDto
    {
        public Guid OrderId { get; set; }
        public Guid RepairRequestId { get; set; }
        public Guid AcceptedOfferId { get; set; }

        public string Status { get; set; } = string.Empty;

        public string ServiceCenterName { get; set; } = string.Empty;
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public int VehicleYear { get; set; }

        public decimal Cost { get; set; }
        public int OriginalDurationInHours { get; set; }
        public int ExtendedDurationInHours { get; set; }
        public string FulfillmentMethod { get; set; } = string.Empty;

        public decimal DeliveryFee { get; set; }
        public decimal? EstimatedDistanceKm { get; set; }
        public decimal TotalCost { get; set; }

        public string? PickupContactName { get; set; }
        public string? PickupContactPhone { get; set; }
        public string? PickupAddressSnapshot { get; set; }

        public decimal? PickupLatitude { get; set; }
        public decimal? PickupLongitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CheckedInAt { get; set; }
        public DateTime? CheckedOutAt { get; set; }
        
        public DateTime? GracePeriodEndsAt { get; set; }
    }
}
