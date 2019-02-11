using System;

namespace Bb.Exceptions
{

    [System.Serializable]
    public class Colis21Exception : Exception
    {
        public Colis21Exception() : base() { }
        public Colis21Exception(string message) : base(message) { }
        public Colis21Exception(string message, System.Exception inner) : base(message, inner) { }
        protected Colis21Exception(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}