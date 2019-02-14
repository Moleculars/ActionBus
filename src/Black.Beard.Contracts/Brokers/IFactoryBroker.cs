using System;
using System.Threading.Tasks;

namespace Bb.Brokers
{

    public interface IFactoryBroker
    {

        /// <summary>
        /// Create broker server from specified configuration server name
        /// </summary>
        /// <param name="publisherName"></param>
        /// <returns></returns>
        IBroker CreateBroker(string serverName);

        /// <summary>
        /// Create publisher from specified configuration key publisher
        /// </summary>
        /// <param name="publisherName"></param>
        /// <returns></returns>
        IBrokerPublisher CreatePublisher(string publisherName);

                /// <summary>
        /// Create subscriber from specified configuration key subscriber
        /// </summary>
        /// <param name="subscriberName"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        IBrokerSubscription CreateSubscription(string subscriberName, Func<IBrokerContext, Task> callback);

    }

}