using Bb.ComponentModel;
using Bb.ComponentModel.Attributes;

namespace Bb.ActionBus.Builders.Brokers
{

    [ExposeClass(ConstantsCore.Configuration, Name = "Brokers", LifeCycle = IocScopeEnum.Singleton)]
    public class BrokerConfiguration
    {

        public BrokerConfiguration()
        {

        }

        public bool Initialize { get; set; }

        public bool Test { get; set; }

        public int TestWaiting { get; set; } = 10;

        public string[] Servers { get; set; }

        public string[] Publishers { get; set; }

        public string[] Subsribers { get; set; }

        public string[] Loggers { get; set; }

    }
}
