using Bb.Brokers;
using Bb.Reminder;
using Bb.ReminderResponse.Broker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.Common;

namespace Bb.ActionBus.Builders.Reminders
{

    public static class ReminderBuilder
    {

        public static void RegisterReminder(this IServiceCollection services, IConfiguration configuration)
        {

            // Initialisation of DbFactory (it is specfic for type 'ReminderStoreSqlServer')
            DbProviderFactories.RegisterFactory(ActionBusBuilderConstants.SqlproviderInvariantName, System.Data.SqlClient.SqlClientFactory.Instance);

            /*
                'Unable to resolve service for type 'Bb.Reminder.IReminderResponseService[]' 
                while attempting to activate 'Bb.Reminder.ReminderService'.'
             */

            services.AddSingleton(typeof(IReminderStore), typeof(ReminderStore));
            services.AddSingleton(typeof(IReminderRequest), typeof(ReminderService));


        }

        public static IServiceProvider  RegisterResponsesReminder(this IServiceProvider services)
        {

            ReminderService reminder = services.GetService(typeof(IReminderRequest)) as ReminderService;
            //ReminderConfiguration configuration = services.GetService(typeof(ReminderConfiguration)) as ReminderConfiguration;
            IFactoryBroker factory = services.GetService(typeof(IFactoryBroker)) as IFactoryBroker;

            reminder.AddResponses(
                new ReminderResponseBroker(factory)
                );

            return services;

        }

    }

}
