using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Bb.Exceptions
{

    /// <summary>
    /// The exception that is thrown when an expression is syntactically invalid 
    /// </summary>
    public class GuardSyntaxErrorException : Colis21Exception
    {
        public List<string> Errors { get; set; } = new List<string>();

        public GuardSyntaxErrorException()
        {
        }

        public GuardSyntaxErrorException(List<string> errors)
        {
            this.Errors = errors;
        }

        public GuardSyntaxErrorException(string message) : base(message)
        {
        }

        public GuardSyntaxErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GuardSyntaxErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
