using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
