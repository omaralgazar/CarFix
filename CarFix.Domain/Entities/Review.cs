using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarFix.Domain.Entities
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid RepairOrderId { get; set; }
        public RepairOrder RepairOrder { get; set; }
        public Guid CustomerId { get; set; }
        public User Customer { get; set; }
        public int CustomerRating { get; set; }
        public int SystemTimeRating { get; set; }
        public decimal CompositeScore => (CustomerRating * 0.5m) + (SystemTimeRating * 0.5m);
        public string? Comment { get; set; }
        public string? CenterReply { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Review() { }
    }
}
