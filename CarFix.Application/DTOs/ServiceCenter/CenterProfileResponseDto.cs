using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.DTOs.ServiceCenter
{
    public class CenterProfileResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public decimal Rating { get; set; }
        public string VerificationStatus { get; set; }

        public List<CenterSpecialtyResponseDto> Specialties { get; set; } = new List<CenterSpecialtyResponseDto>();
    }
}
