using System.Collections.Generic;

namespace Bb.Contracts
{
    /// <summary>
    /// Representation of a message coming from a message broker.
    /// Low-level interface - most of the time BasicMessage will be used instead inside modules.
    /// </summary>
    public interface IBrokerMessage
    {
        object TransactionId { get; }

        string Utf8Data { get; }

        string RoutingKey { get; }

        /// <summary>
        /// A message may have headers. (can be null or empty).
        /// </summary>
        IDictionary<string, object> Headers { get; set; }

        /// <summary>
        /// Latest message read is marked as correctly read and should never be presented again (may actually happen).
        /// </summary>
        void Commit();

        /// <summary>
        /// Discard a message, never present it again.
        /// </summary>
        void Reject();

        /// <summary>
        /// Will put the message back in the queue, at the start of the queue.
        /// </summary>
        void RequeueLast();

        /// <summary>
        /// Dioscard a message, represent it later.
        /// </summary>
        void Rollback();
    }
}
