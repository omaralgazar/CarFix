using CarFix.Application.DTOs.ServiceCenter;
using CarFix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.DTOs.Admin
{
    public class AdminServiceCenterResponseDto
    {
        public Guid CenterId { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public string CenterAddress { get; set; } = string.Empty;
        public string CenterPhone { get; set; } = string.Empty;

        public Guid OwnerUserId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;

        public List<CenterCapabilityResponseDto> Capabilities { get; set; }
            = new();
    }
}
