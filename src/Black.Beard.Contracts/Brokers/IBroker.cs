﻿using System;
using System.Threading.Tasks;

namespace Bb.Brokers
{
    /// <summary>
    /// Entry point to the message broker.
    /// </summary>
    public interface IBroker : IDisposable
    {

        /// <summary>
        /// Register a new subscription to an existing queue (i.e. on the default exchange)
        /// </summary>
        /// <param name="subscriptionParameters"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        IBrokerSubscription Subscribe(object subscriptionParameters, Func<IBrokerContext, Task> callback);

        /// <summary>
        /// Get a new instance of a publisher on an exchange.
        /// </summary>
        /// <param name="brokerPublishParameters"></param>
        /// <returns>A ready to publish publisher</returns>
        IBrokerPublisher GetPublisher(object brokerPublishParameters);

        /// <summary>
        /// Remove all data from broker.
        /// </summary>
        Task Reset();

        /// <summary>
        /// Message count in queue in internal broker. 0 if queue does not exist.
        /// </summary>
        /// <param name="queueName"></param>
        Task<int> GetQueueDepth(string queueName);

        ///// <summary>
        ///// Declare a queue without bindings. Used only in tests.
        ///// </summary>
        ///// <param name="queueName"></param>
        ///// <param name="durable"></param>
        ///// <param name="exclusive"></param>
        ///// <param name="autoDelete"></param>
        //void QueueDeclare(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false);

        /// <summary>
        /// Bind a queue to a topic. Used only in tests.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="exchangeName"></param>
        void BindTopic(string queueName, string exchangeName);

    }
}
