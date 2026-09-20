using CarFix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarFix.Domain.Entities
{
    public class ScopeChangeRequest
    {
        public Guid Id { get; set; }
        public Guid RepairOrderId { get; set; }
        public RepairOrder RepairOrder { get; set; }
        public string Description { get; set; }
        public string? ImageUrls { get; set; }
        public decimal ExtraCost { get; set; }
        public int ExtraDurationInHours { get; set; }
        public ScopeChangeStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ScopeChangeRequest() { }
    }
}
