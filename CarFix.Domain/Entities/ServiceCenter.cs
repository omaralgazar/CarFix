using CarFix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarFix.Domain.Entities
{
    public class ServiceCenter
    {
        public Guid Id { get; set; }
        public Guid OwnerUserId { get; set; }
        public User Owner { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public decimal Rating { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        public bool IsBanned { get; set; }
        public int ConsecutiveDelayCount { get; set; }
        public int BanEscalationCount { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CenterCapability> Capabilities { get; set; } = new List<CenterCapability>();
        public ICollection<RepairOffer> Offers { get; set; } = new List<RepairOffer>();

        public ServiceCenter() { }

    }
}
