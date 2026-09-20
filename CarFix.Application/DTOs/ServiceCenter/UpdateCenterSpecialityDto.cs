using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.DTOs.ServiceCenter
{
    public class UpdateCenterSpecialityDto
    {
        public string? Type { get; set; } = string.Empty;  // مثال: "Brand"
        public string? Value { get; set; } = string.Empty;
    }
}
