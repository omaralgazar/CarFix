using CarFix.Application.Configuration;
using CarFix.Application.Interfaces;
using Microsoft.Extensions.Options;
using Resend;
using CarFix.Infrastructure.Email;
using System.Net;
using System.Net.Mail;

namespace CarFix.Infrastructure.Email
{
    public class ResendEmailSender : IEmailSender
    {
        private readonly IResend _resend;
        private readonly ResendSettings _settings;

        public ResendEmailSender(
            IResend resend,
            IOptions<ResendSettings> settings)
        {
            _resend = resend;
            _settings = settings.Value;
        }

        public async Task SendPasswordResetEmailAsync(
    string email,
    string resetToken)
        {
            var safeToken = WebUtility.HtmlEncode(resetToken);

            var message = new EmailMessage
            {
                From = $"{_settings.FromName} <{_settings.FromEmail}>",
                Subject = "Reset your CarFix password",
                HtmlBody = $"""
            <h2>Reset your password</h2>
            <p>We received a request to reset your CarFix password.</p>
            <p>Use this code in the reset-password request:</p>
            <h3>{safeToken}</h3>
            <p>This code expires in 15 minutes.</p>
            <p>If you did not request this, you can safely ignore this email.</p>
            """
            };

            message.To.Add(email);

            await _resend.EmailSendAsync(message);
        }
    }
}