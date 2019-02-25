using System;

namespace FailToBan.Core
{
    public class VicException : Exception
    {
        public VicException(string message) : base(message)
        { }
    }
}