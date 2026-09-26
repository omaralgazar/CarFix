using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.DTOs.ServiceCenter.RepairOfferDto
{
    public class UpdateOfferDto
    {
        public decimal Cost { get; set; }
        public int DurationInHours { get; set; }
    }
}
