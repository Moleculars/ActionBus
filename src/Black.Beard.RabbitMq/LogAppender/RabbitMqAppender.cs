using Bb.Brokers;
using Bb.Core.ComponentModel;
using Bb.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading;

namespace Bb.LogAppender
{
    /// <summary>
    /// Custom appender for RabbitMQ. 
    /// </summary>
    public class RabbitMqAppender : TraceListener
    {

        public static RabbitMqAppender Initialize(RabbitBrokers self, string arguments)
        {
            var logger = new RabbitMqAppender(self, arguments);
            System.Diagnostics.Trace.Listeners.Add(logger);
            return logger;
        }

        private RabbitMqAppender(RabbitBrokers self, string arguments)
        {
            ConnectionStringHelper.Map(this, arguments);
            _publisher = self.CreatePublisher(PublisherName);
            _timer = new Timer(AppendAsync, null, AppendLogsIntervalSeconds * 1000, AppendLogsIntervalSeconds * 1000);
        }

        /// <summary>
        /// Interval in seconds between appending/sending the logs (default 2 sec.)
        /// used by a timer
        /// </summary>
        public int AppendLogsIntervalSeconds { get; set; } = 2;

        [Required]
        public string PublisherName { get; set; }

        public HashSet<string> Tracks { get; set; }

        #region Dispose

        /// <summary>
        /// When appender is closed, dispose Broker and Timer
        /// </summary>
        protected void OnClose()
        {
            _publisher?.Dispose();
            _timer?.Dispose();
        }

        public override void Flush()
        {
            base.Flush();

            try
            {
                AppendAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("could not flush appender " + e.Message + " - log messages may have been lost.\n" + e.StackTrace); // do not use logger here, as we are inside the logger...
            }
        }

        protected override void Dispose(bool disposing)
        {
            Flush();
            base.Dispose(disposing);
            _publisher?.Dispose();
            _timer?.Dispose();
        }

        #endregion

        #region override

        public override void Write(string message)
        {
            Log(message, TraceLevel.Info.ToString());
        }

        public override void WriteLine(string message)
        {
            Log(message, TraceLevel.Info.ToString());
        }

        public override void Write(object o)
        {
            Log(o, TraceLevel.Info.ToString());
        }

        public override void Fail(string message)
        {
            Log(message, TraceLevel.Error.ToString());
        }

        public override void Fail(string message, string detailMessage)
        {
            //Log(message, detailMessage);
        }

        public override void Write(object o, string category)
        {
            Log(o, category);
        }

        public override void WriteLine(object o)
        {
            Log(o, TraceLevel.Error.ToString());
        }

        public override void Write(string message, string category)
        {
            Log(message, category);
        }

        public override void WriteLine(object o, string category)
        {
            Log(o, category);
        }

        public override void WriteLine(string message, string category)
        {
            Log(message, category);
        }

        private void Log(object message, string category)
        {

            if (message is string txt)
                _bufferQueue.Enqueue($"{{ Message : {message}, Level : {category} }}");

            else
            {

                var properties = DictionnarySerializerExtension.GetDictionnaryProperties(message, true);
                properties.Add("Category", category);

                var sb = new System.Text.StringBuilder(1000);

                properties.SerializeObjectToJson(sb, false);

                _bufferQueue.Enqueue(sb.ToString());

            }

        }

        #endregion override

        /// <summary>
        /// AsyncMethod to append/send logs to rabbitMQ. Adds a new Task for each message to be published
        /// and commits and waits for all to finish. The maximum amount of publish per queue is set to 1000.
        /// </summary>
        /// <returns>Empty Task</returns>
        private void AppendAsync(object state = null)
        {

            while (!inTreatment && _bufferQueue.Count > 0)
                PushOutAsynch();

            //Console.WriteLine($"stoped push log {Name}");

        }

        private void PushOutAsynch()
        {

            lock (_lock) // general lock - only one thread reads the queue and sends messages at a time.
            {
                inTreatment = true;
                try
                {
                    Console.WriteLine($"Starting push log {Name}");
                    var logList = new List<string>(_bufferQueue.Count + 10);
                    var queue = (_publisher as RabbitBrokerPublisher).BrokerPublishParameters.DefaultQueue;

                    if (_bufferQueue.Count > 0)
                    {

                        try
                        {

                            while (_bufferQueue.TryDequeue(out var log))    // dequeue all logs from queue
                            {
                                logList.Add(log);
                                if (logList.Count > MAX_BROKER_LINES)
                                    break;
                            }

                            if (logList.Count > 0)
                                _publisher.Publish(routingKey: queue, message: string.Format("[{0}]", string.Join(",", logList)));

                        }
                        catch (Exception e1)
                        {

                            // In case of error, just give up the backlog and restart logger. Print on stdout (do not use the logger!)
                            Console.WriteLine(e1.ToString());
                            _bufferQueue = new ConcurrentQueue<string>();

                            try
                            {
                                _publisher?.Rollback();
                            }
                            catch (Exception)
                            {
                                // Do nothing.
                            }
                        }
                    }

                }
                finally
                {
                    inTreatment = false;
                }
            }

        }

        private static readonly int MAX_BROKER_LINES = 1000;
        //private readonly BrokerPublishParameters _cnxBroker;

        /// <summary>
        /// (single threaded) persistent publisher, holding the broker session.
        /// </summary>
        private IBrokerPublisher _publisher;

        /// <summary>
        /// Buffer queue containing the string (of loggingEvent) to be sent/appended to rabbitMQ
        /// </summary>
        private ConcurrentQueue<string> _bufferQueue = new ConcurrentQueue<string>();

        /// <summary>
        /// Timer to execute the async appending of logs
        /// </summary>
        private readonly Timer _timer;
        private readonly object _lock = new object();
        private bool inTreatment = false;
    }
}
