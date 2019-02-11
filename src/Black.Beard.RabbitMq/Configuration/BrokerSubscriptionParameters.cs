using Bb.Helpers;
using System.Collections.Generic;

namespace Bb.Configuration
{

    /// <summary>
    /// Describes what basically amounts to a persistent AMQP subscription
    /// </summary>
    public class BrokerSubscriptionParameters : BrokerParameters
    {

        public BrokerSubscriptionParameters(string connectionString)
        {
            ConnectionStringHelper.Map<BrokerSubscriptionParameters>(this, connectionString);
        }

        /// <summary>
        /// The routing keys to accept (the "binding keys"). Leave empty for all. Ignored for default exchange.
        /// </summary>
        public List<string> AcceptedRoutingKeys { get; set; } = new List<string>(10);

        /// <summary>
        /// The subscription yields messages which are stored in a queue before they can be consumed by the client message processor.
        /// </summary>
        public string StorageQueueName { get; set; }

        /// <summary>
        /// True: subscription stays on even if the consumers are dead.
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// For a single process (there may be many processes!) never process more than MaxParallelism messages concurrently.
        /// </summary>
        public int MaxParallelism { get; set; } = 2;

        /// <summary>
        /// How many times the message may be requeued before being labeled a poison message. -1 to disable.
        /// </summary>
        public int MaxReplayCount { get; set; } = 20;

        /// <summary>
        /// The header used for the poison message mechanism.
        /// </summary>
        public string ReplayHeaderKey { get; set; } = "REPLAY";
    }
}
