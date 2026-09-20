using CarFix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces
{
    public interface ITokenGenerator
    {
        string GenerateToken(User user);
    }

}
