using CarFix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarFix.Domain.Entities
{
    public class Dispute
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public RepairOrder Order { get; set; }
        public Guid RaisedByUserId { get; set; }
        public User RaisedBy { get; set; }
        public Guid? StaffId { get; set; }
        public User? Staff { get; set; }
        public string Reason { get; set; }
        public string? Resolution { get; set; }
        public DisputeStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
