using Bb.Configuration;
using Bb.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bb.Broker
{

    public class RabbitBrokers
    {

        public RabbitBrokers()
        {
            _serverConfigurations = new Dictionary<string, ServerBrokerConfiguration>();
            _brokerPublishConfigurations = new Dictionary<string, BrokerPublishParameters>();
            _brokerSubscriptionConfigurations = new Dictionary<string, BrokerSubscriptionParameters>();
        }



        public void AddServer(string configuration)
        {
            var server = new ServerBrokerConfiguration(configuration);
            AddServer(server);
        }

        public void AddServer(ServerBrokerConfiguration server)
        {
            _serverConfigurations.Add(server.Name, server);
        }



        public void AddPublisher(string configuration)
        {
            var server = new BrokerPublishParameters(configuration);
            AddPublisher(server);
        }

        public void AddPublisher(BrokerPublishParameters server)
        {
            _brokerPublishConfigurations.Add(server.Name, server);
        }



        public void AddSubscriptionBroker(string configuration)
        {
            var server = new BrokerSubscriptionParameters(configuration);
            AddSubscriptionBroker(server);
        }

        public void AddSubscriptionBroker(BrokerSubscriptionParameters subscriptionConfiguration)
        {
            _brokerSubscriptionConfigurations.Add(subscriptionConfiguration.Name, subscriptionConfiguration);
        }



        public IBrokerPublisher CreatePublisher(string serverName, string publisherName)
        {

            if (!_serverConfigurations.TryGetValue(serverName, out ServerBrokerConfiguration server))
                throw new Exceptions.InvalidConfigurationException($"configuration server {serverName}");

            if (!_brokerPublishConfigurations.TryGetValue(publisherName, out BrokerPublishParameters publisher))
                throw new Exceptions.InvalidConfigurationException($"configuration publisher {serverName}");

            var _broker = new RabbitBroker(server);
            var _publisher = _broker.GetPublisher(publisher);

            return _publisher;

        }

        public IBrokerSubscription CreateSubscription(string serverName, string publisherName, Func<IBrokerMessage, Task> callback)
        {

            if (!_serverConfigurations.TryGetValue(serverName, out ServerBrokerConfiguration server))
                throw new Exceptions.InvalidConfigurationException($"configuration server {serverName}");

            if (!_brokerSubscriptionConfigurations.TryGetValue(publisherName, out BrokerSubscriptionParameters subscriberParameter))
                throw new Exceptions.InvalidConfigurationException($"configuration subscription {serverName}");

            IBroker _broker = new RabbitBroker(server);
            var _Subscriber = _broker.Subscribe(subscriberParameter, callback);

            return _Subscriber;

        }

        private Dictionary<string, ServerBrokerConfiguration> _serverConfigurations;
        private Dictionary<string, BrokerPublishParameters> _brokerPublishConfigurations;
        private Dictionary<string, BrokerSubscriptionParameters> _brokerSubscriptionConfigurations;

    }

}
