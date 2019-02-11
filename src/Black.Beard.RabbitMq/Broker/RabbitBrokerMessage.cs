using Bb.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.Text;

namespace Bb.Broker
{
    class RabbitBrokerMessage : IBrokerMessage
    {
        internal BasicDeliverEventArgs Message { private get; set; }
        internal IModel Session { private get; set; }

        public object TransactionId => Message.DeliveryTag;

        public string Utf8Data => Encoding.UTF8.GetString(Message.Body);

        public string RoutingKey => Message.RoutingKey;

        public IDictionary<string, object> Headers
        {
            get => Message.BasicProperties.Headers;
            set => Message.BasicProperties.Headers = value;
        }

        public void Commit()
        {
            Session.BasicAck(Message.DeliveryTag, false);
        }

        public void Reject()
        {
            Session.BasicReject(Message.DeliveryTag, false);
        }

        public void Rollback()
        {
            Session.BasicNack(Message.DeliveryTag, false, true);
        }

        public void RequeueLast()
        {
            Session.BasicPublish(Message.Exchange, Message.RoutingKey, Message.BasicProperties, Message.Body);
            Commit();
        }
    }
}
