using CarFix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarFix.Domain.Entities
{
    public class CenterSpecialty
    {
        public Guid Id { get; set; }
        public Guid ServiceCenterId { get; set; }
        public ServiceCenter ServiceCenter { get; set; }
        public SpecialtyType Type { get; set; }
        public string Value { get; set; }

    }
}
