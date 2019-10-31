using Bb.Brokers;
using Bb.Exceptions;
using Bb.Helpers;

namespace Bb.ActionBus.Builders.Brokers
{

    public class Broker : RabbitFactoryBrokers
    {

        public Broker(BrokerConfiguration configuration)
        {

            var servers = configuration.Servers 
                ?? throw new InvalidConfigurationException("no servers broker defined in the configuration");

            foreach (var server in servers)
                this.AddServerFromConnectionString(server);

            if (configuration.Publishers != null)
                foreach (var publisher in configuration.Publishers)
                    this.AddPublisherFromConnectionString(publisher);

            if (configuration.Subsribers != null)
                foreach (var subsriber in configuration.Subsribers)
                    this.AddSubscriberFromConnectionString(subsriber);

            if (configuration.Initialize)
            {

                this.Initialize();

                if (configuration.Test)
                {
                    this.Test(out bool success, configuration.TestWaiting);
                    if (!success)
                        throw new Bb.Exceptions.InvalidConfigurationException("invalid borker configuration, please inspect log");
                }

            }

            foreach (var logger in configuration.Loggers)
            {
                var config = new RabbitBrokerLog();
                if (!ConnectionStringHelper.Map<RabbitBrokerLog>(config, logger, false))
                    throw new InvalidConfigurationException($"Failed to load configuration {logger}");
                Bb.LogAppender.RabbitMqAppender.Initialize(this, config.Name, config.PublisherName, config.Reformat, config.AppendLogsIntervalSeconds, config.Tracks.ToArray());
            }

        }

    }

}
