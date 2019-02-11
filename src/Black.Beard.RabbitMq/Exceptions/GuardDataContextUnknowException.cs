using System;
using System.Runtime.Serialization;

namespace Bb.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an expression refer to an unknow run-time context properties.
    /// </summary>
    public class GuardDataContextUnknowException : Colis21Exception
    {
        public GuardDataContextUnknowException()
        {
        }

        public GuardDataContextUnknowException(string message) : base(message)
        {
        }

        public GuardDataContextUnknowException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GuardDataContextUnknowException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
