using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.DTOs.RepairOrder
{
    public class OtpResponseDto
    {
        public string OtpCode { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
