using Bb.ActionBus.Builders.Brokers;
using Bb.ActionBus.Builders.Reminders;
using Bb.Brokers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bb.ActionBus.Builders
{

    public class ActionBusBuilder : IBuilder
    {

        public ActionBusBuilder()
        {

        }

        public void Initialize(IServiceCollection services, IConfiguration configuration)
        {

            services.AddSingleton(typeof(IFactoryBroker), typeof(Broker));

            configuration.RegisterCustomCode();
            services.RegisterReminder(configuration);
            services.CreateActionRepositories();
            services.CreateSubscriptionInstances();

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

             app.ApplicationServices
                .RegisterResponsesReminder()
                .RegisterBusinessActions(
                    typeof(Reminder.ReminderAction)     // business action for remind action
                )
                .RegisterListeners();

        }

    }
}
