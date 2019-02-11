using Bb.Configuration;
using Bb.Contracts;
using Bb.Exceptions;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bb.Broker
{
    internal sealed class RabbitBrokerPublisher : IBrokerPublisher
    {

        public RabbitBrokerPublisher(RabbitBroker broker, BrokerPublishParameters brokerPublishParameters)
        {
            _broker = broker;
            BrokerPublishParameters = brokerPublishParameters;
            ServerConfiguration = _broker.Configuration;
        }

        public BrokerPublishParameters BrokerPublishParameters { get; }

        public ServerBrokerConfiguration ServerConfiguration { get; }


        public Task Publish(object message, IDictionary<string, object> headers = null)
        {
            return Publish(null, message, headers);
        }

        public Task Publish(string routingKey, object message, IDictionary<string, object> headers = null)
        {

            Initialize();

            // Message prep
            var stringMessage = this.BrokerPublishParameters.JsonConversion ? JsonConvert.SerializeObject(message, _jsonSerializationSettings) : message.ToString();
            var data = Encoding.UTF8.GetBytes(stringMessage);

            // Deliver to exchange
            var props = _session.CreateBasicProperties();
            props.DeliveryMode = (byte)this.BrokerPublishParameters.DeliveryMode;
            if (headers != null)
            {
                props.Headers = new Dictionary<string, object>(headers.Count);
                foreach (var pair in headers)
                    props.Headers[pair.Key] = pair.Value;
            }
            _session.ContinuationTimeout = _defaultContinuationTimeout;
            _session.BasicPublish(this.BrokerPublishParameters.ExchangeName ?? "", routingKey ?? this.BrokerPublishParameters.DefaultRoutingKey, props, data);
            return Task.CompletedTask;
        }

        public void BeginTransaction()
        {

            Initialize();

            if (_txOpen)
                throw new IllegalStateException("a rabbit MQ TX is already open on this session");

            _txOpen = true;
            _session.ContinuationTimeout = TimeSpan.FromSeconds(60);
            _session.TxSelect();

        }

        public void Commit()
        {
            _session.TxCommit();
            _txOpen = false;
        }

        public void Rollback()
        {
            if (_session != null && _session.IsOpen)
                _session.TxRollback();
            _txOpen = false;
        }

        public void Dispose()
        {
            if (_txOpen)
            {
                _session?.TxRollback();
                _txOpen = false;
            }
            _session?.Close();
            _session?.Dispose();
            _session = null;
            _initialized = false;
        }

        private void Initialize()
        {
            if (_initialized)
            {
                if (_session.IsClosed)
                {
                    _session = _broker.GetChannel();
                }
            }
            else
            {
                _session = _broker.GetChannel();

                if (_broker.Configuration.ConfigAllowed)
                {

                    if (!string.IsNullOrWhiteSpace(this.BrokerPublishParameters.ExchangeName))
                        _session.ExchangeDeclare(this.BrokerPublishParameters.ExchangeName, this.BrokerPublishParameters.ExchangeType.ToString().ToLower(), true, false);

                    if (string.IsNullOrWhiteSpace(this.BrokerPublishParameters.ExchangeName) && !string.IsNullOrWhiteSpace(this.BrokerPublishParameters.DefaultRoutingKey))
                        // Direct exchange, default routing key => direct queue publishing, so create the queue.
                        _session.QueueDeclare(this.BrokerPublishParameters.DefaultRoutingKey, true, false, false);

                }

                _initialized = true;
            }
        }


        private readonly RabbitBroker _broker;
        private IModel _session;
        private bool _initialized = false;
        private bool _txOpen = false;
        private readonly TimeSpan _defaultContinuationTimeout = TimeSpan.FromSeconds(60);
        private readonly JsonSerializerSettings _jsonSerializationSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

    }
}
