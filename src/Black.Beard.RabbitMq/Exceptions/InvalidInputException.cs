using System;

namespace Bb.Exceptions
{
    public class InvalidInputException : Colis21Exception
    {
        public InvalidInputException(string message) : base(message) { }
    }
}