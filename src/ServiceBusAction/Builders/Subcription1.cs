using Bb.ActionBus;
using Bb.Brokers;
using System;
using System.Threading.Tasks;

namespace ServiceBusAction.Builders
{

    public class Subcription1 : SubscriptionInstance
    {

        public Subcription1(string name, IFactoryBroker brokers, string subscriberName, ActionRepositories actionRepositories)
            : base(name)
        {
            _actionRepositories = actionRepositories;
            Subscription = brokers.CreateSubscription(subscriberName, context => Callback(context));
        }

        private Task Callback(IBrokerContext context)
        {

            ActionOrder order = ActionOrder.Unserialize(context.Utf8Data);

            if (_actionRepositories.Execute(order))
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

                _acknowledgeQueue.Publish(
                    new
                    {
                        Order = order,
                        order.ExecutedAt,
                        Result
                    }
                );

                context.Commit();

            }
            else
            {

                if (context.CanBeRequeued())
                    context.RequeueLast();

                else
                {

                    var Exception = order.Result as Exception;
                    _deadQueue.Publish(
                        new
                        {
                            Order = order,
                            order.ExecutedAt,
                            order.PushedAt,
                            Exception
                        }
                    );

                    context.Reject();

                }

            }

            return Task.CompletedTask;

        }


        private readonly ActionRepositories _actionRepositories;

        public IBrokerPublisher _acknowledgeQueue { get; internal set; }

        public IBrokerPublisher _deadQueue { get; internal set; }

    }


}
