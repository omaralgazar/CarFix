using CarFix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarFix.Domain.Entities
{
    public class RepairOffer
    {
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public RepairRequest Request { get; set; }
        public Guid CenterId { get; set; }
        public ServiceCenter Center { get; set; }
        public Decimal Cost { get; set; } = Decimal.Zero;
        public int DurationInHours { get; set; } = 0;
        public decimal GracePeriodHours => Math.Min(DurationInHours * 0.2m, 24);
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public RepairOfferStatus Status { get; set; }
        public RepairOffer() { }
    }
}
