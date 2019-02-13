using Bb.Configuration;
using Bb.Exceptions;
using EasyNetQ.Management.Client;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Bb.Brokers
{

    /// <summary>
    /// Main entry point for all RabbitMQ operations
    /// </summary>
    public sealed class RabbitBroker : IBroker
    {

        public RabbitBroker(ServerBrokerConfiguration configuration)
        {

            Configuration = configuration;

            if (Configuration == null)
            {
                Trace.WriteLine("the broker is used without configuration", TraceLevel.Error.ToString());
                throw new InvalidOperationException("the broker is used without configuration");
            }
        }

        public IModel GetChannel()
        {
            Init();
            return _connection.CreateModel();
        }

        private Uri GetEndpoint()
        {
            // Sessions (i.e. channels, i.e. IModel) are created from the connexion, but are not thread safe.
            var endPoint = Configuration.Hostname.LocalhostHandling();
            Uri uri = new Uri($"amqp://{endPoint}:{Configuration.Port}");
            return uri;
        }

        private void Init()
        {

            if (_connection == null)
                lock (this)
                    if (_connection == null)    // Create the connection
                    {
 
                        var rabbitMqFactory = new ConnectionFactory()
                        {
                            Uri = GetEndpoint(),
                            UserName = Configuration.UserName,
                            Password = Configuration.Password,
                            AutomaticRecoveryEnabled = true,
                            NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                            RequestedHeartbeat = 5,
                            RequestedConnectionTimeout = 20000,
                            ContinuationTimeout = TimeSpan.FromSeconds(60),
                        };

                        _connection = CreateConnectionWithTimeout(rabbitMqFactory);

                        // Test connection
                        using (var channel = GetChannel())
                        {
                            // Nothing to do.
                        }

                    }

        }

        private ManagementClient Manager()
        {

            if (Configuration.ConfigAllowed && Configuration.ManagementPort.HasValue)
            {
                try
                {
                    _managmentClient = new ManagementClient(_connection.Endpoint.HostName, Configuration.UserName, Configuration.Password, Configuration.ManagementPort.Value);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(new { Message = "Failed to create _managment client", Exception = e }, TraceLevel.Error.ToString());
                    throw;
                }

            }

            return _managmentClient;

        }

        public async Task Reset()
        {

            Init();

            var manager = Manager();

            if (manager == null)
                throw new IllegalStateException("cannot purge RabbitMQ broker without a management connection");

            await Task.WhenAll((await manager.GetQueuesAsync()).Select(q => manager.PurgeAsync(q)));
            await Task.WhenAll((await manager.GetBindingsAsync()).Where(q => q.DestinationType == "queue" && !string.IsNullOrEmpty(q.RoutingKey) && !string.IsNullOrEmpty(q.Source)).Select(q => manager.DeleteBindingAsync(q)));

        }

        private IConnection CreateConnectionWithTimeout(ConnectionFactory rabbitMqFactory)
        {
            IConnection connection = null;
            if (Configuration.UseLogger)
                Trace.WriteLine($"Attempting to connect to RabbitMQ with connectionString: {rabbitMqFactory.Uri}", TraceLevel.Info.ToString());

            if (Configuration.ConnectionTimeoutSeconds > 0)
            {
                var limit = DateTime.Now.AddSeconds(Configuration.ConnectionTimeoutSeconds);
                while (connection == null && DateTime.Now <= limit)
                {
                    try
                    {
                        connection = rabbitMqFactory.CreateConnection();
                    }
                    catch (BrokerUnreachableException e)
                    {
                        if (Configuration.UseLogger)
                            Trace.WriteLine(new { Message = $"Could not connect to RabbitMQ broker. Will retry in {Configuration.ConnectionRetryIntervalSeconds} seconds.", Exception = e }, TraceLevel.Error.ToString());

                        // sniff
                        Thread.Sleep(1000 * Configuration.ConnectionRetryIntervalSeconds);

                    }
                }

                if (connection == null)
                {
                    if (Configuration.UseLogger)
                        Trace.WriteLine("Giving up on opening a connection to the RabbitMQ broker", TraceLevel.Error.ToString());
                    else
                        Console.WriteLine("Giving up on opening a connection to the RabbitMQ broker");

                    throw new TransientFailureException($"Cannot reach broker on {GetEndpoint().ToString()}");
                }

                if (Configuration.UseLogger)
                    Trace.WriteLine("Successfully connected to RabbitMQ", TraceLevel.Info.ToString());

                return connection;
            }
            else
            {
                return rabbitMqFactory.CreateConnection();
            }
        }

        public IBrokerSubscription Subscribe(object subscriptionParameters, Func<IBrokerContext, Task> callback)
        {

            BrokerSubscriptionParameters _subscriptionParameters = (subscriptionParameters as BrokerSubscriptionParameters) ?? throw new InvalidConfigurationException("subscriptionParameters must be of type BrokerSubscriptionParameters");

            var res = new RabbitBrokerSubscription();
            res.Subscribe(this, _subscriptionParameters, callback);
            return res;
        }

        public IBrokerPublisher GetPublisher(object brokerPublishParameters)
        {
            BrokerPublishParameters _brokerPublishParameters = brokerPublishParameters as BrokerPublishParameters ?? throw new InvalidConfigurationException("brokerPublishParameters must be of type BrokerPublishParameters");
            return new RabbitBrokerPublisher(this, _brokerPublishParameters);
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_connection != null)
                {
                    _connection?.Close();
                    _connection = null;
                }
            }
        }

        ///// <summary>
        ///// Declare a simple queue bound on the default exchange with routing key = queue name.
        ///// </summary>
        ///// <param name="queueName"></param>
        //public void QueueDeclare(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false)
        //{
        //    using (var channel = GetChannel())
        //    {
        //        channel.QueueDeclare(queueName, durable, exclusive, autoDelete, null);
        //    }
        //}

        public Task<int> GetQueueDepth(string queueName)
        {
            using (var channel = GetChannel())
            {
                try
                {
                    return Task.FromResult((int)channel.MessageCount(queueName));
                }
                catch (Exception)
                {
                    return Task.FromResult(0);
                }
            }
        }


        /// <summary>
        /// Global RabbitMQ configuration (from config file).
        /// </summary>
        public ServerBrokerConfiguration Configuration { get; private set; }

        /// <summary>
        /// If true, all queues will be purged on startup. Only used in tests.
        /// </summary>
        public bool PurgeBrokerOnStartup { get; set; } = false;

        //public Task<BasicMessage> GetMessageInQueue(string queueName)
        //{
        //    using (var session = GetChannel())
        //    {
        //        var result = session.BasicGet(queueName, true);
        //        if (result != null)
        //        {
        //            var messageId = result.BasicProperties.MessageId;
        //            var data = System.Text.Encoding.UTF8.GetString(result.Body);
        //            return Task.FromResult(new BasicMessage { Headers = result.BasicProperties.Headers, Id = messageId, Utf8Data = data });
        //        }
        //    }

        //    return Task.FromResult<BasicMessage>(null);
        //}

        public void BindTopic(string queueName, string exchangeName)
        {
            using (var channel = GetChannel())
            {
                channel.QueueDeclare(queueName, true, false, false, null);
                channel.ExchangeDeclare(exchangeName, "topic", true, false, null);
                channel.QueueBind(queueName, exchangeName, "*", null);
            }
        }

        private IConnection _connection;
        private ManagementClient _managmentClient;

    }

    public static class RabbitBrokerEngineBuilderExtensions
    {

        //public static IModuleHostBuilder UseBrokerRabbit(this IModuleHostBuilder builder, bool purgeOnStartup = false)
        //{

        //    // INTERNAL
        //    builder.InternalMessageBroker = new RabbitBroker(builder..BootstrapConfiguration.Rabbit) { PurgeBrokerOnStartup = purgeOnStartup };

        //    // EXTERNAL
        //    if (builder.BootstrapConfiguration.Modules.TryGetValue("ModuleHost", out var hostConfig) &&
        //        hostConfig.AdditionalParameters.TryGetValue("rabbitExternalConnectionString", out var cnStr))
        //    {
        //        hostConfig.AdditionalParameters.TryGetValue("rabbitExternalUserName", out var username);
        //        hostConfig.AdditionalParameters.TryGetValue("rabbitExternalPassword", out var password);

        //        var config = new RabbitConfiguration
        //        {
        //            ConnexionString = cnStr,
        //            Password = password,
        //            UserName = username,
        //            ManagementPort = null,
        //            ConfigAllowed = false
        //            //TODO: check if used. QosPrefetchCount
        //        };

        //        builder.ExternalMessageBroker = new RabbitBroker(config);
        //    }
        //    else
        //    {
        //        builder.ExternalMessageBroker = builder.InternalMessageBroker;
        //    }

        //    return builder;
        //}


        /// <summary>
        /// The connection to a rabbit broker/network of brokers. Thread safe.
        /// </summary>

    }

    internal static class ConnectionHelper
    {

        public static string LocalhostHandling(this string s)
        {
            if (s.ToLower() == "localhost")
                return GetDefaultLocalIp();
            return s;
        }

        /// <summary>
        /// Select the IP of the first non-loopback interface with a valid IPv4 gateway.
        /// </summary>
        private static string GetDefaultLocalIp()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback && n.GetIPProperties()?.GatewayAddresses?.Count > 0)
                .SelectMany(i => i.GetIPProperties().UnicastAddresses)
                .Where(a => a?.Address != null && a.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.Address)
                .FirstOrDefault()?.ToString() ?? "localhost";
        }
    }


}
