using Bb.Brokers;
using Bb.Configuration;
using Bb.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics;


namespace ServiceBusAction.Builders
{

    public static class LoggerBuilderExtensions
    {

        public static void RegisterLogToBrokers(this IConfiguration configuration, RabbitFactoryBrokers brokers)
        {

            var loggers = Collectkeys(configuration, "Loggers");

            foreach (var item in loggers)
            {
                var appender = Bb.LogAppender.RabbitMqAppender.Initialize(brokers, item);
                Trace.WriteLine($"Logger {appender.Name} initialized");
            }

        }

        public static RabbitFactoryBrokers RegisterBrokers(this IServiceCollection services, IConfiguration configuration)
        {

            var broker = new RabbitFactoryBrokers();

            var servers = Collectkeys(configuration, "Servers");
            if (servers.Count > 0)
            {
                foreach (var server in servers)
                    broker.AddServer(server);
            }
            else
                Trace.WriteLine($"sample broker server : {ConnectionStringHelper.GenerateHelp(typeof(ServerBrokerConfiguration))}");

            var publishers = Collectkeys(configuration, "Publishers");
            if (publishers.Count > 0)
            {
                foreach (var publisher in publishers)
                    broker.AddPublisher(publisher);
            }
            else
                Trace.WriteLine($"sample broker publisher : {ConnectionStringHelper.GenerateHelp(typeof(BrokerPublishParameters))}");


            var subsribers = Collectkeys(configuration, "Subsribers");
            if (subsribers.Count > 0)
            {
                foreach (var subsriber in subsribers)
                    broker.AddSubscriptionBroker(subsriber);
            }
            else
                Trace.WriteLine($"sample broker subscriber : {ConnectionStringHelper.GenerateHelp(typeof(BrokerSubscriptionParameters))}");

            services.AddSingleton(typeof(IFactoryBroker), broker);

            return broker;

        }

        private static List<string> Collectkeys(IConfiguration configuration, string key)
        {

            List<string> folders = new List<string>();

            for (int i = 0; i < 1000; i++)
            {
                var folder = configuration.GetValue<string>($"{key}:{i}");
                if (!string.IsNullOrEmpty(folder))
                    folders.Add(folder);
                else
                {
                    if (i == 0)
                        Trace.WriteLine($"node '{key}' not found in the configuration.", TraceLevel.Warning.ToString());
                    break;
                }
            }

            if (folders.Count == 0)
                Trace.WriteLine($"no config {key} injected.", TraceLevel.Warning.ToString());

            return folders;

        }

    }

}
