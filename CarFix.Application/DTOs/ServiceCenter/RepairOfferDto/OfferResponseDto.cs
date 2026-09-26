using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.DTOs.ServiceCenter.RepairOfferDto
{
    public class OfferResponseDto
    {
        public Guid Id { get; set; }
        public Guid RepairRequestId { get; set; }
        public Guid CenterId { get; set; }
        public string CenterName { get; set; }
        public decimal CenterRating { get; set; }
        public decimal Cost { get; set; }
        public int DurationInHours { get; set; }
        public decimal GracePeriodHours { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
