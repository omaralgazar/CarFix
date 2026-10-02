using CarFix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarFix.Domain.Entities
{
    public class RepairRequest 
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public User Customer { get; set; }
        public Guid VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public string IssueCategory { get; set; }
        public string IssueDescription { get; set; }
        public string? ImageUrls { get; set; }
        public RepairRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public FulfillmentMethod FulfillmentMethod { get; set; }
        public Guid? PickupAddressId { get; set; }
        public UserAddress? PickupAddress { get; set; }

        public string? PickupContactName { get; set; }
        public string? PickupContactPhone { get; set; }
        public string? PickupAddressSnapshot { get; set; }

        public decimal? PickupLatitude { get; set; }
        public decimal? PickupLongitude { get; set; }

        public ICollection<RepairOffer> Offers { get; set; } = new List<RepairOffer>();
        public RepairOrder? Order { get; set; }

        public RepairRequest() { }
    }
}
