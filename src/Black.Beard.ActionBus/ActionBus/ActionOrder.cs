using System;
using System.Collections.Generic;

namespace Bb.ActionBus
{

    public class ActionOrder
    {

        public ActionOrder()
        {
            this.Arguments = new List<ArgumentOrder>();
        }

        public Guid Uuid { get; set; }

        public DateTimeOffset PushedAt { get; set; }

        public string Name { get; set; }
        public List<ArgumentOrder> Arguments { get; set; }

        public DateTimeOffset ExecuteStarted { get; internal set; }

        public DateTimeOffset ExecuteStoped { get; internal set; }

        public object Result { get; internal set; }

    }

}
