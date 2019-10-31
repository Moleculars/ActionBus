using Bb.ActionBus;
using Bb.Brokers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Bb.ActionBus.Builders.Brokers
{

    public static partial class ListenerBuilderExtensions
    {


        public static void CreateSubscriptionInstances(this IServiceCollection services)
        {
            services.AddSingleton(typeof(SubscriptionInstances));
        }


        public static IServiceProvider RegisterListeners(this IServiceProvider serviceProvider)
        {

            SubscriptionInstances subscriptions = serviceProvider.GetService(typeof(SubscriptionInstances)) as SubscriptionInstances;
            IFactoryBroker brokers = serviceProvider.GetService(typeof(IFactoryBroker)) as IFactoryBroker;
            IConfiguration configuration = serviceProvider.GetService(typeof(IConfiguration)) as IConfiguration;
            ActionRepositories actionRepositories = serviceProvider.GetService(typeof(ActionRepositories)) as ActionRepositories;

            var subscriberName = configuration.GetValue<string>("listener");
            Subcription1 sub = new Subcription1("Actions", brokers, subscriberName, actionRepositories)
            {
                _acknowledgeQueue = brokers.CreatePublisher(configuration.GetValue<string>("AcknowledgeQueue")),
                _deadQueue = brokers.CreatePublisher(configuration.GetValue<string>("DeadQueue")),
            };

            subscriptions.AddSubscription(sub);

            return serviceProvider;

        }


    }

}
