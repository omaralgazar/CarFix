using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        (bool IsValid, bool NeedsRehash) VerifyPassword(string hashedPassword, string providedPassword);
    }
}
