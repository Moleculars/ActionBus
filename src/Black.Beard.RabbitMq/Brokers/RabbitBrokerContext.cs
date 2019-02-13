using Bb.Configuration;
using Bb.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.Text;

namespace Bb.Brokers
{
    public class RabbitBrokerContext : IBrokerContext
    {

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="parameters"></param>
        internal RabbitBrokerContext(BrokerSubscriptionParameters parameters)
        {
            _parameters = parameters;
        }

        public object TransactionId => Message.DeliveryTag;

        /// <summary>
        /// Return the message from utf8. 
        /// </summary>
        public string Utf8Data => Encoding.UTF8.GetString(Message.Body);

        /// <summary>
        /// The exchange the message was originally published to
        /// </summary>
        public string Exchange => Message.Exchange;

        /// <summary>
        /// The routing key used when the message was originally published.
        /// </summary>
        public string RoutingKey => Message.RoutingKey;

        /// <summary>
        /// A message may have headers. (can be null or empty).
        /// </summary>
        public IDictionary<string, object> Headers
        {
            get => Message.BasicProperties.Headers;
            set => Message.BasicProperties.Headers = value;
        }

        /// <summary>
        /// Latest message read is marked as correctly read and should never be presented again (may actually happen).
        /// </summary>
        public void Commit()
        {
            Session.BasicAck(Message.DeliveryTag, false);
        }

        /// <summary>
        /// Discard a message, never present it again.
        /// </summary>
        public void Reject()
        {
            Session.BasicReject(Message.DeliveryTag, false);
        }

        /// <summary>
        /// Discard a message, represent it later.
        /// </summary>
        public void Rollback()
        {
            Session.BasicNack(Message.DeliveryTag, false, true);
        }

        /// <summary>
        /// Will put the message back in the queue, at the start of the queue.
        /// </summary>
        public void RequeueLast()
        {

            if (Message.BasicProperties.Headers.TryGetValue(_parameters.ReplayHeaderKey, out object o))
            {

                if (int.TryParse(o.ToString(), out int count))
                    count++;
                else
                    count = 2;

                if (count > _parameters.MaxReplayCount)
                    throw new MaxReplayException(_parameters.MaxReplayCount, this);

                Message.BasicProperties.Headers[_parameters.ReplayHeaderKey] = count;
            }
            else
                Message.BasicProperties.Headers.Add(_parameters.ReplayHeaderKey, 1);

            Session.BasicPublish(Message.Exchange, Message.RoutingKey, Message.BasicProperties, Message.Body);

            Commit();

        }

        /// <summary>
        /// How many times the message may be requeued before being labeled a poison message. -1 to disable.
        /// </summary>
        public int MaxReplayCount => _parameters.MaxReplayCount;

        internal BasicDeliverEventArgs Message { private get; set; }
        internal IModel Session { private get; set; }
        private readonly BrokerSubscriptionParameters _parameters;


    }
}
