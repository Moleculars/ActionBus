using Bb.ActionBus;
using Bb.Brokers;
using System;
using System.Threading.Tasks;

namespace ServiceBusAction.Builders
{

    public class Subcription1 : SubscriptionInstance
    {

        public Subcription1(string name, RabbitBrokers brokers, string subscriberName, ActionRepositories actionRepositories)
            : base(name)
        {
            _actionRepositories = actionRepositories;
            Subscription = brokers.CreateSubscription(subscriberName, context => Callback(context));
        }

        private Task Callback(IBrokerContext context)
        {

            ActionOrder order = ActionOrder.Unserialize(context.Utf8Data);

            int count = 0;
            if (context.Headers.TryGetValue("REPLAY", out object valueHeader))
                count = int.Parse(valueHeader.ToString());

            if (_actionRepositories.Execute(order, count))
            {

                string Result = string.Empty;
                switch (order.Name)
                {

                    case "business1.PushScan":
                        Result = ((Guid)order.Result).ToString("B");
                        break;

                    default:
                        Result.ToString();
                        break;

                }

                _acknowledgeQueue.Publish(new
                {
                    Order = order,
                    order.ExecutedAt,
                    Result
                });


                context.Commit();

            }
            else
            {

                var exception = order.Result as Exception;

                _deadQueue.Publish(
                new
                {
                    Order = order,
                    order.ExecutedAt,
                    order.PushedAt,
                    exception
                }
                );

            }

            return Task.CompletedTask;

        }


        private readonly ActionRepositories _actionRepositories;

        public IBrokerPublisher _acknowledgeQueue { get; internal set; }

        public IBrokerPublisher _deadQueue { get; internal set; }

    }


}
