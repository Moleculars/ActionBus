using Bb.Brokers;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace ServiceBusAction.Builders
{

    public static class ListenerBuilderExtensions
    {

        public static void RegisterListeners(this IConfiguration configuration, RabbitBrokers brokers)
        {

            var subscription = brokers.CreateSubscription(configuration.GetValue<string>("listener"), context =>
            {



               return Task.CompletedTask;

            });


        }


    }

}
