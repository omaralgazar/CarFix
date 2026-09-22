using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.DTOs.Customer.RepairRequest
{
    public class CreateRepairRequestDto
    {
        public Guid VehicleId { get; set; }
        public string IssueCategory { get; set; }
        public string IssueDescription { get; set; }
        public string? ImageUrls { get; set; }
    }
}
