using Bb.Configuration;
using Bb.Core.ComponentModel;
using Bb.Exceptions;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bb.Brokers
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

        /// <summary>
        /// Push a new message in the broker. the default queue from the publisher configuration is use to be pushed in the broker.
        /// </summary>
        /// <param name="queue">queue name</param>
        /// <param name="message">message object. if the message is <see cref="string"/> or <see cref="StringBuilder"/>, it is pushed without transformation.</param>
        /// <param name="headers">can be a dictionnary<string, object> or a anonymous class.If it is a anonymous class it all properties are translated in dictionnary<string, object></param>
        /// <returns></returns>
        public Task Publish(object message, object headers = null)
        {
            var h = TranslateHeaders(headers);
            return Publish_Impl(null, message, h);
        }

        /// <summary>
        /// Push a new message in the broker
        /// </summary>
        /// <param name="queue">queue name</param>
        /// <param name="message">message object. if the message is <see cref="string"/> or <see cref="StringBuilder"/>, it is pushed without transformation.</param>
        /// <param name="headers">can be a dictionnary<string, object> or a anonymous class.If it is a anonymous class it all properties are translated in dictionnary<string, object></param>
        /// <returns></returns>
        public Task Publish(string queue, object message, object headers = null)
        {
            var h = TranslateHeaders(headers);
            return Publish_Impl(queue, message, h);
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Dictionary<string, object> TranslateHeaders(object headers)
        {

            Dictionary<string, object> _headers = null;

            if (headers != null)
            {
                if (headers is IDictionary<string, object> d)
                {
                    _headers = new Dictionary<string, object>(d.Count);
                    foreach (var pair in d)
                        _headers[pair.Key] = pair.Value;
                }
                else
                    _headers = DictionnarySerializerExtension.GetDictionnaryProperties(headers);
            }

            return _headers;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] BuildMessage(object message)
        {
            string stringMessage = string.Empty;
            if (message is string || message is StringBuilder)
                stringMessage = message.ToString();
            else
                stringMessage = JsonConvert.SerializeObject(message, _jsonSerializationSettings);
            var data = Encoding.UTF8.GetBytes(stringMessage);
            return data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Task Publish_Impl(string queue, object message, Dictionary<string, object> headers = null)
        {

            Initialize();

            byte[] data = BuildMessage(message);        // Message prep

            // Deliver to exchange
            var props = _session.CreateBasicProperties();
            props.DeliveryMode = (byte)BrokerPublishParameters.DeliveryMode;
            if (headers != null)
                props.Headers = headers;

            _session.ContinuationTimeout = _defaultContinuationTimeout;
            _session.BasicPublish(BrokerPublishParameters.ExchangeName ?? string.Empty, queue ?? BrokerPublishParameters.DefaultQueue, props, data);

            return Task.CompletedTask;

        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Initialize()
        {

            if (!_initialized)
                lock (_lock)
                    if (!_initialized)
                    {

                        _session = _broker.GetChannel();

                        if (_broker.Configuration.ConfigAllowed)
                        {

                            if (!string.IsNullOrWhiteSpace(BrokerPublishParameters.ExchangeName))
                                _session.ExchangeDeclare(BrokerPublishParameters.ExchangeName, BrokerPublishParameters.ExchangeType.ToString().ToLower(), true, false);

                            if (string.IsNullOrWhiteSpace(BrokerPublishParameters.ExchangeName) && !string.IsNullOrWhiteSpace(BrokerPublishParameters.DefaultQueue))
                                // Direct exchange, default routing key => direct queue publishing, so create the queue.
                                _session.QueueDeclare(BrokerPublishParameters.DefaultQueue, true, false, false);

                        }

                        _initialized = true;

                    }

            if (_session.IsClosed)
                _session = _broker.GetChannel();

        }

        private readonly RabbitBroker _broker;
        private IModel _session;
        private bool _initialized = false;
        private bool _txOpen = false;
        private readonly TimeSpan _defaultContinuationTimeout = TimeSpan.FromSeconds(60);
        private readonly object _lock = new object();

        private readonly JsonSerializerSettings _jsonSerializationSettings = new JsonSerializerSettings
        {

            Formatting = System.Diagnostics.Debugger.IsAttached
                            ? Formatting.Indented
                            : Formatting.None,

            NullValueHandling = NullValueHandling.Ignore,

        };

    }
}
