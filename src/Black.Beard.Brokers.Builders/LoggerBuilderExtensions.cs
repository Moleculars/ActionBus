using Bb.Brokers;
using Bb.Configurations;
using Bb.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ServiceBusAction.Builders
{

    public static class LoggerBuilderExtensions
    {

        public static void RegisterLogToBrokers(this List<object> _instances)
        {

            IServiceCollection services = _instances.First(c => (c is IServiceCollection)) as IServiceCollection;
            IConfiguration configuration = _instances.First(c => (c is IConfiguration)) as IConfiguration;
            IFactoryBroker brokers = _instances.First(c => (c is IFactoryBroker)) as IFactoryBroker;

            var loggers = Collectkeys(configuration, "Loggers");

            foreach (string item in loggers)
            {

                RabbitBrokerLog p = new RabbitBrokerLog();
                if (!ConnectionStringHelper.Map<RabbitBrokerLog>(p, item, false))
                    throw new Bb.Exceptions.InvalidConfigurationException($"parameters can't be mapped from '{item}'");

                var appender = Bb.LogAppender.RabbitMqAppender.Initialize(brokers, p.Name, p.PublisherName, p.Reformat, p.AppendLogsIntervalSeconds, p.Tracks.ToArray());

                Trace.WriteLine($"Logger {appender.Name} initialized");

            }

        }

        public static RabbitFactoryBrokers RegisterBrokers(this List<object> _instances)
        {

            IServiceCollection services = _instances.First(c => (c is IServiceCollection)) as IServiceCollection;
            IConfiguration configuration = _instances.First(c => (c is IConfiguration)) as IConfiguration;

            var broker = new RabbitFactoryBrokers();

            var servers = Collectkeys(configuration, "Servers");
            if (servers.Count > 0)
                foreach (var server in servers)
                    broker.AddServerFromConnectionString(server);

            else
                Trace.WriteLine($"sample broker server : {ConnectionStringHelper.GenerateHelp(typeof(ServerBrokerConfiguration))}");

            var publishers = Collectkeys(configuration, "Publishers");
            if (publishers.Count > 0)
                foreach (var publisher in publishers)
                    broker.AddPublisherFromConnectionString(publisher);

            else
                Trace.WriteLine($"sample broker publisher : {ConnectionStringHelper.GenerateHelp(typeof(BrokerPublishParameter))}");


            var subsribers = Collectkeys(configuration, "Subsribers");
            if (subsribers.Count > 0)
                foreach (var subsriber in subsribers)
                    broker.AddSubscriberFromConnectionString(subsriber);

            else
                Trace.WriteLine($"sample broker subscriber : {ConnectionStringHelper.GenerateHelp(typeof(BrokerSubscriptionParameter))}");

            services.AddSingleton(typeof(IFactoryBroker), broker);

            _instances.Add(broker);

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

    public class RabbitBrokerLog
    {

        public RabbitBrokerLog()
        {
            Tracks = new List<string>();
        }

        public string Name { get; set; }

        public string PublisherName { get; set; }

        public bool Reformat { get; set; }

        public List<string> Tracks { get; set; }

        public int AppendLogsIntervalSeconds { get; set; }

    }

}
