﻿using Bb.Configuration;
using Bb.Core.Helpers;
using Bb.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Bb.Brokers
{

    internal sealed class RabbitBrokerSubscription : IBrokerSubscription
    {

        public RabbitBrokerSubscription()
        {
        }

        /// <summary>
        /// subscribe with custom business delegate
        /// </summary>
        /// <param name="broker"></param>
        /// <param name="parameters"></param>
        /// <param name="callback"></param>
        internal void Subscribe(RabbitBroker broker, BrokerSubscriptionParameters parameters, Func<IBrokerContext, Task> callback)
        {

            _broker = broker;
            _parameters = parameters;
            _callback = callback;
            _watcher = parameters.WatchDog;

            CreateSession();

        }

        public void CreateSession()
        {
            if (_session != null)
                throw new BrokerException("subscripber is allready initialized");

            _session = new Session(_watcher, _broker, _parameters, _callback);

            _timer = new Timer(Append, null, _watcher * 1000, _watcher * 1000);

        }

        /// <summary>
        /// AsyncMethod to append/send logs to rabbitMQ. Adds a new Task for each message to be published
        /// and commits and waits for all to finish. The maximum amount of publish per queue is set to 1000.
        /// </summary>
        /// <returns>Empty Task</returns>
        private async void Append(object state = null)
        {
            if (_session != null)
            {
                var status = _session.Status();
                if (status.Item1 < Clock.GetNow)
                {
                    var count = await _broker.GetQueueDepth(_parameters.StorageQueueName);
                    if (count > 0)
                        Trace.WriteLine("network cut detected", TraceLevel.Error.ToString());
                }
            }
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            _timer.Dispose();
            _session?.Dispose();
            _session = null;
            _timer = null;
        }

        private class Session
        {

            public Session(int watcher, RabbitBroker broker, BrokerSubscriptionParameters parameters, Func<IBrokerContext, Task> callback)
            {

                _nextWarnTime = Clock.GetNow.AddSeconds((watcher * 2) + 1);

                _broker = broker;
                _parameters = parameters;
                _callback = callback;
                _watcher = watcher;

                _callback = callback;
                _session = broker.GetChannel();
                _session.BasicQos(0, (ushort)parameters.MaxParallelism, false);

                var _consumer = new EventingBasicConsumer(_session);
                _consumer.Received += _consumer_Received;

                _act = () =>
                {
                    _consumer.Received -= _consumer_Received;
                };

                if (broker.Configuration.ConfigAllowed)
                    SetUpBindings(parameters, _session);

                _session.BasicConsume(parameters.StorageQueueName, false, _consumer);

            }

            public void Dispose()
            {
                Close();
            }


            public (DateTimeOffset, long) Status()
            {
                var count = Interlocked.Read(ref _count);
                return (_nextWarnTime, count);
            }

            public void Close()
            {
                if (_session != null)
                {

                    _act();

                    var maxWait = Clock.GetNow.AddSeconds(_parameters.MaxTimeWaitingToClose);
                    while (Interlocked.Read(ref _count) > 0)
                        if (maxWait < Clock.GetNow)
                        {
                            Trace.WriteLine($"interrupt waiting closure subcriber {this._parameters.Name}");
                            break;
                        }
                    _session.Close();
                    _session = null;
                }
            }

            private async void _consumer_Received(object sender, BasicDeliverEventArgs e)
            {

                lock (_lock)
                    _nextWarnTime = Clock.GetNow.AddSeconds((_watcher * 2) + 1);

                Interlocked.Increment(ref _count);

                await _callback(new RabbitBrokerContext(_parameters)
                {
                    Message = e,
                    Session = _session
                });

                Interlocked.Decrement(ref _count);

            }

            private void SetUpBindings(BrokerSubscriptionParameters parameters, IModel channel)
            {

                // Create the receiving queue.
                channel.QueueDeclare(parameters.StorageQueueName, parameters.Durable, false, !parameters.Durable);

                // No binding to create. Just create the queue, as bindings are implicit on the default exchange.                
                if (string.IsNullOrWhiteSpace(parameters.ExchangeName))
                    return;

                // Normal case - exchange -> binding -> queue.
                channel.ExchangeDeclare(parameters.ExchangeName, parameters.ExchangeType.ToString().ToLowerInvariant(), true, false);
                foreach (var routingKey in parameters.AcceptedRoutingKeys)
                    channel.QueueBind(parameters.StorageQueueName, parameters.ExchangeName, routingKey);

            }

            private readonly object _lock = new object();
            private readonly Func<IBrokerContext, Task> _callback;
            private readonly int _watcher;
            private readonly Action _act;
            private readonly RabbitBroker _broker;
            private readonly BrokerSubscriptionParameters _parameters;

            private long _count = 0;
            private DateTimeOffset _nextWarnTime;
            private IModel _session;

        }

        private Session _session;
        private RabbitBroker _broker;
        private BrokerSubscriptionParameters _parameters;
        private Func<IBrokerContext, Task> _callback;
        private int _watcher;
        private Timer _timer;

    }
}


