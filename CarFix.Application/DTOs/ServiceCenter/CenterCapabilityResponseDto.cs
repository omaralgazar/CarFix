using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.DTOs.ServiceCenter
{
    public class CenterCapabilityResponseDto
    {
        public Guid Id { get; set; }
        public string IssueCategory { get; set; } = string.Empty;
        public string? VehicleBrand { get; set; }
    }
}
