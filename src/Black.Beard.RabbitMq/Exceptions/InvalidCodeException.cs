using System;

namespace Bb.Exceptions
{
    /// <summary>
    /// Thrown when the programmer has made a mistake (SDK misuse notably).
    /// </summary>
    [Serializable]
    public class InvalidCodeException : Colis21Exception
    {
        public InvalidCodeException(string message) : base(message) { }

        public InvalidCodeException(string message, Exception e) : base(message, e) { }
    }
}
