using System;

namespace Bb.Exceptions
{
    [System.Serializable]
    public class TransientFailureException : Colis21Exception
    {
        public TransientFailureException() : base() { }

        public TransientFailureException(string message) : base(message) { }
        public TransientFailureException(string message, Exception e) : base(message, e) { }
    }
}
