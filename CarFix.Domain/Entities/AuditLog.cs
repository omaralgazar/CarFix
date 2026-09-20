using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarFix.Domain.ConstantsClass;

namespace CarFix.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public Guid StaffId { get; set; }
        public User Staff { get; set; }
        public string Action { get; set; }
        public string EntityName { get; set; }
        public Guid EntityId { get; set; }
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
