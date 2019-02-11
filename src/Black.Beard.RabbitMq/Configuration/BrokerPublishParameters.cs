using Bb.Helpers;
using Bb.RabbitMq;

namespace Bb.Configuration
{
    /// <summary>
    /// Describe how to publish messages on an AMQP exchange.
    /// </summary>
    public class BrokerPublishParameters : BrokerParameters
    {

        public BrokerPublishParameters(string connectionString)
        {
            ConnectionStringHelper.Map<BrokerPublishParameters>(this, connectionString);
        }

        /// <summary>
        /// Whether the messages published will be persistent between reboots or not.
        /// </summary>
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.Persistent;

        /// <summary>
        /// Set to true to have the broker automatically serialize the messages to JSON.
        /// If false, the message will be be sent as-is.
        /// </summary>
        public bool JsonConversion { get; set; } = true;

        /// <summary>
        /// If no routing key is specified at publish time, use this one. Can be null.
        /// Especially useful for default exchange publishers aimed at a specific queue.
        /// </summary>
        public string DefaultRoutingKey { get; set; } = null;

    }

}
