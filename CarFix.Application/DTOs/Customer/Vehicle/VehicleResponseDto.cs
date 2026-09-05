using System;
using System.Collections.Generic;
using System.Text;


namespace CarFix.Application.DTOs.Customer.Vehicle
{
    public class VehicleResponseDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }
        public bool IsDefault { get; set; }
        public VehicleResponseDto() { }
       
    }

}
