using System;
using System.Threading.Tasks;
using Bb.Configuration;
using Bb.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Bb.Broker
{

    internal sealed class RabbitBrokerSubscription : IBrokerSubscription
    {

        private IModel _session;

        private Func<IBrokerMessage, Task> _callback;

        internal void Subscribe(RabbitBroker broker, BrokerSubscriptionParameters parameters, Func<IBrokerMessage, Task> callback)
        {
            _callback = callback;
            _session = broker.GetChannel();
            _session.BasicQos(0, (ushort)parameters.MaxParallelism, false);

            var _consumer = new EventingBasicConsumer(_session);
            _consumer.Received += _consumer_Received;

            if (broker.Configuration.ConfigAllowed)
            {
                SetUpBindings(parameters, _session);
            }

            _session.BasicConsume(parameters.StorageQueueName, false, _consumer);
        }

        private void SetUpBindings(BrokerSubscriptionParameters parameters, IModel channel)
        {
            // Create the receiving queue.
            channel.QueueDeclare(parameters.StorageQueueName, parameters.Durable, false, !parameters.Durable);

            if (string.IsNullOrWhiteSpace(parameters.ExchangeName))
            {
                // No binding to create. Just create the queue, as bindings are implicit on the default exchange.                
                return;
            }

            // Normal case - exchange -> binding -> queue.
            channel.ExchangeDeclare(parameters.ExchangeName, parameters.ExchangeType.ToString().ToLowerInvariant(), true, false);
            foreach (var routingKey in parameters.AcceptedRoutingKeys)
            {
                channel.QueueBind(parameters.StorageQueueName, parameters.ExchangeName, routingKey);
            }
        }

        private async void _consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            await _callback(new RabbitBrokerMessage
            {
                Message = e,
                Session = _session
            });
        }

        public void Dispose()
        {
            _session?.Close();
        }
    }
}
