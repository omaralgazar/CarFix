using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.DTOs.Customer.RepairRequest
{
    public class RepairRequestResponseDto
    {
        public Guid Id { get; set; }
        public Guid VehicleId { get; set; }
        public string VehicleModel { get; set; }
        public string IssueCategory { get; set; }
        public string IssueDescription { get; set; }
        public string? ImageUrls { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
    }
}
