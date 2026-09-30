using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CarFix.Application.DTOs.RepairOrder
{
    public class ConfirmCheckInDto
    {
        [Required]
        [StringLength(4, MinimumLength = 4)]
        public string OtpCode { get; set; } = string.Empty;
    }
}
