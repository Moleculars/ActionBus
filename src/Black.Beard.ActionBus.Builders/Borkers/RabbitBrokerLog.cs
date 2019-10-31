using Bb.Brokers;
using Bb.Helpers;
using Bb.LogAppender;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bb.ActionBus.Builders.Brokers
{

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
