using System;

namespace Bb.Exceptions
{
    [System.Serializable]
    public class TransientFailureException : BrokerException
    {
        public TransientFailureException() : base() { }

        public TransientFailureException(string message) : base(message) { }
        public TransientFailureException(string message, Exception e) : base(message, e) { }
    }
}
