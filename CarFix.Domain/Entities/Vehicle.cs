using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarFix.Domain.Entities
{
    public class Vehicle
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public User Customer { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }
        public bool IsDefault { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<RepairRequest> RepairRequests { get; set; } = new List<RepairRequest>();

        public Vehicle() { }
        
    }
}
