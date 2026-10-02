using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Domain.Entities
{
    public class ServiceCenterImages 
    {
        public Guid Id { get; set; }

        public Guid ServiceCenterId { get; set; }
        public ServiceCenter ServiceCenter { get; set; } = null!;

        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
