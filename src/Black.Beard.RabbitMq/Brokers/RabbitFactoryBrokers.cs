using Bb.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bb.Brokers
{

    public class RabbitFactoryBrokers : IFactoryBroker
    {

        public RabbitFactoryBrokers()
        {
            _serverConfigurations = new Dictionary<string, ServerBrokerConfiguration>();
            _brokerPublishConfigurations = new Dictionary<string, BrokerPublishParameters>();
            _brokerSubscriptionConfigurations = new Dictionary<string, BrokerSubscriptionParameters>();
        }


        #region configuration

        /// <summary>
        /// Append a new configuration server
        /// </summary>
        /// <param name="configuration"></param>
        public void AddServer(string configuration)
        {
            var server = new ServerBrokerConfiguration(configuration);
            AddServer(server);
        }

        /// <summary>
        /// Append a new configuration server
        /// </summary>
        /// <param name="server"></param>
        public void AddServer(ServerBrokerConfiguration server)
        {
            _serverConfigurations.Add(server.Name, server);
        }

        /// <summary>
        /// Append a new configuration publisher
        /// </summary>
        /// <param name="configuration"></param>
        public void AddPublisher(string configuration)
        {
            var server = new BrokerPublishParameters(configuration);
            AddPublisher(server);
        }

        /// <summary>
        /// Append a new configuration publisher
        /// </summary>
        /// <param name="server"></param>
        public void AddPublisher(BrokerPublishParameters server)
        {
            _brokerPublishConfigurations.Add(server.Name, server);
        }


        /// <summary>
        /// Append a new configuration subscriber
        /// </summary>
        /// <param name="configuration"></param>
        public void AddSubscriptionBroker(string configuration)
        {
            var server = new BrokerSubscriptionParameters(configuration);
            AddSubscriptionBroker(server);
        }

        /// <summary>
        /// Append a new configuration subscriber
        /// </summary>
        /// <param name="subscriptionConfiguration"></param>
        public void AddSubscriptionBroker(BrokerSubscriptionParameters subscriptionConfiguration)
        {
            _brokerSubscriptionConfigurations.Add(subscriptionConfiguration.Name, subscriptionConfiguration);
        }

        #endregion configuration


        /// <summary>
        /// Create publisher from specified configuration key publisher
        /// </summary>
        /// <param name="publisherName"></param>
        /// <returns></returns>
        public IBrokerPublisher CreatePublisher(string publisherName)
        {


            if (!_brokerPublishConfigurations.TryGetValue(publisherName, out BrokerPublishParameters publisher))
                throw new Exceptions.InvalidConfigurationException($"configuration publisher {publisherName}");

            if (!_serverConfigurations.TryGetValue(publisher.ServerName, out ServerBrokerConfiguration server))
                throw new Exceptions.InvalidConfigurationException($"configuration server {publisher.ServerName}");

            var _broker = new RabbitBroker(server);
            var _publisher = _broker.GetPublisher(publisher);

            return _publisher;

        }

        /// <summary>
        /// Create broker server from specified configuration server name
        /// </summary>
        /// <param name="publisherName"></param>
        /// <returns></returns>
        public IBroker CreateBroker(string serverName)
        {

            if (!_serverConfigurations.TryGetValue(serverName, out ServerBrokerConfiguration server))
                throw new Exceptions.InvalidConfigurationException($"configuration server {serverName}");

            var _broker = new RabbitBroker(server);

            return _broker;

        }

        /// <summary>
        /// Create subscriber from specified configuration key subscriber
        /// </summary>
        /// <param name="subscriberName"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IBrokerSubscription CreateSubscription(string subscriberName, Func<IBrokerContext, Task> callback)
        {

            if (!_brokerSubscriptionConfigurations.TryGetValue(subscriberName, out BrokerSubscriptionParameters subscriberParameter))
                throw new Exceptions.InvalidConfigurationException($"configuration subscription {subscriberName}");

            if (!_serverConfigurations.TryGetValue(subscriberParameter.ServerName, out ServerBrokerConfiguration server))
                throw new Exceptions.InvalidConfigurationException($"configuration server {subscriberParameter.ServerName}");

            IBroker _broker = new RabbitBroker(server);
            var _Subscriber = _broker.Subscribe(subscriberParameter, callback);

            return _Subscriber;

        }

        private Dictionary<string, ServerBrokerConfiguration> _serverConfigurations;
        private Dictionary<string, BrokerPublishParameters> _brokerPublishConfigurations;
        private Dictionary<string, BrokerSubscriptionParameters> _brokerSubscriptionConfigurations;

    }

}
