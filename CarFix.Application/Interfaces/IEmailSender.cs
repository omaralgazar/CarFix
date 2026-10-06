using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces
{
    
        public interface IEmailSender
        {
            Task SendPasswordResetEmailAsync(
                string email,
                string resetToken);
        }
    
}
