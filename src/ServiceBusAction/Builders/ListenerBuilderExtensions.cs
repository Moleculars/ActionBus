using Bb.ActionBus;
using Bb.Brokers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ServiceBusAction.Builders
{

    public static partial class ListenerBuilderExtensions
    {

        public static void RegisterListeners(this IConfiguration configuration, IServiceCollection services, RabbitBrokers brokers, ActionRepositories actionRepositories)
        {

            var subscriptions = new SubscriptionInstances(brokers);

            var subscriberName = configuration.GetValue<string>("listener");
            Subcription1 sub = new Subcription1("Actions", brokers, subscriberName, actionRepositories)
            {
                _acknowledgeQueue = brokers.CreatePublisher(configuration.GetValue<string>("AcknowledgeQueue")),
                _deadQueue = brokers.CreatePublisher(configuration.GetValue<string>("DeadQueue")),
            };

            subscriptions.AddSubscription(sub);         

            services.AddSingleton(subscriptions);

        }


    }

}
