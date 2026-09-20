using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.DTOs.AuthDto
{
    public class RegisterCenterDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }

    }
}
