using Bb.Helpers;

namespace Bb.Configuration
{

    public class ServerBrokerConfiguration
    {

        public ServerBrokerConfiguration(string connectionString)
        {

            ConnectionStringHelper.Map(this, connectionString);



        }

        public string Name { get; set; }

        /// <summary>
        /// Broker connection string, like amqp://lapin:5672
        /// </summary>
        public string ConnexionString { get; set; }

        /// <summary>
        /// Configure prefetch count  for Rabbit Qos
        /// </summary>
        public ushort QosPrefetchCount { get; set; }

        /// <summary>
        /// Colis21 should make connection attempts for this long. It crashes if 
        /// no connections could be created within this timeframe.
        /// If 0, only one attempt is done.
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 120;

        /// <summary>
        /// How often we should attempt to connect.
        /// </summary>
        public int ConnectionRetryIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// RabbitMQ login.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// RabbitMQ password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The number of max replay count
        /// </summary>
        public int MaxReplayCount { get; set; } = 20;

        ///// <summary>
        ///// The name of the key, set in the message header when replaying
        ///// </summary>
        //////public string ReplayKeyHeader { get; set; } = "REPLAY";

        public bool UseLogger { get; set; } = true;

        /// <summary>
        /// Only needed for purging queues. Null means management is disabled. Set to null in production.
        /// </summary>
        public int? ManagementPort { get; set; } = 15672;

        /// <summary>
        /// Set to false if the user does not have permissions to change broker configuration
        /// </summary>
        public bool ConfigAllowed { get; set; } = true;

    }
}
