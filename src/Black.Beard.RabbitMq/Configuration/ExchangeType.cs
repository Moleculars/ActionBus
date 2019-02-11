namespace Bb.Configuration
{
    /// <summary>
    /// The different AMQP exchange types we allow.
    /// </summary>
    public enum ExchangeType
    {
        DIRECT,
        TOPIC,
        /// <summary>
        /// DO NOT USE - for compatibility with shared rabbitmq library only.
        /// </summary>
        FANOUT
    }
}
