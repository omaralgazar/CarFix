using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CarFix.Application.DTOs.Customer.Address
{
    public class UserAddressResponseDto
    {
        public Guid Id { get; set; }

        public string Label { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;

        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public bool IsDefault { get; set; }
    }
}
