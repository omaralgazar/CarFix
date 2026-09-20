using CarFix.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<object> _hasher = new();

        public string HashPassword(string password) =>
            _hasher.HashPassword(null!, password);

        public (bool IsValid, bool NeedsRehash) VerifyPassword(string hashedPassword, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);

            return result switch
            {
                PasswordVerificationResult.Success => (true, false),
                PasswordVerificationResult.SuccessRehashNeeded => (true, true),
                _ => (false, false)
            };
        }
    
}
}
