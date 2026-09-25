using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.DTOs.ServiceCenter.RepairOfferDto
{
    public class CreateOfferDto
    {
        public Guid RepairRequestId { get; set; }
        public decimal Cost { get; set; }
        public int DurationInHours { get; set; }
    }
}
