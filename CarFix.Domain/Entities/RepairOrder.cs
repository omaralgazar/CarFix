using CarFix.Domain.Enums;
using System;
using System.Collections.Generic;

namespace CarFix.Domain.Entities
{
    public class RepairOrder
    {
        public Guid Id { get; set; }
        public Guid RepairRequestId { get; set; }
        public RepairRequest RepairRequest { get; set; }
        public Guid AcceptedOfferId { get; set; }
        public RepairOffer AcceptedOffer { get; set; }
        public string? CheckInOTP { get; set; }
        public string? CheckOutOTP { get; set; }
        public DateTime? CheckInOTPExpiry { get; set; }
        public DateTime? CheckOutOTPExpiry { get; set; }
        public DateTime? CheckedInAt { get; set; }
        public DateTime? CheckedOutAt { get; set; }
        public DateTime? GracePeriodEndsAt { get; set; }
        public int OriginalDurationInHours { get; set; }
        public int ExtendedDurationInHours { get; set; }
        public RepairOrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ScopeChangeRequest> ScopeChangeRequests { get; set; } = new List<ScopeChangeRequest>();
        public ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();
        public Invoice? Invoice { get; set; }
        public Review? Review { get; set; }

        public RepairOrder() { }
    }
}