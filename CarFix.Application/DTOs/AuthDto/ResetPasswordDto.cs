using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.DTOs.AuthDto
{
    public class ResetPasswordDto
    {
        public string Token { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;

        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
