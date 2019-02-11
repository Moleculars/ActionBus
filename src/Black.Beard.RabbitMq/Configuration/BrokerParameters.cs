using Bb.RabbitMq;

namespace Bb.Configuration
{
    public class BrokerParameters
    {

        public string Name { get; set; }


        /// <summary>
        /// Which broker should we use: the internal one or the public one?
        /// </summary>
        public BrokerType BrokerType { get; set; } = BrokerType.Locked;

        /// <summary>
        /// Exchange to bind to. Empty/null for default exchange.
        /// </summary>
        public string ExchangeName { get; set; } = null;

        /// <summary>
        /// Exchange type. Used to create the exchange if it does not exist and to interpret the routing keys.
        /// </summary>
        public ExchangeType ExchangeType { get; set; } = ExchangeType.DIRECT;

        public ServerBrokerConfiguration Server { get; internal set; }

    }
}
