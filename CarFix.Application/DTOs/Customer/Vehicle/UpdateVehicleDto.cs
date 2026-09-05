using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.DTOs.Customer.Vehicle
{
    public class UpdateVehicleDto
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }


    }
}
