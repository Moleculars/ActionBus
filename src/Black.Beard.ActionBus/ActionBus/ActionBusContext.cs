using System;

namespace Bb.ActionBus
{

    public class ActionBusContext
    {

        public object Result { get; internal set; }

        public ActionOrder Order { get; internal set; }

        public Func<ActionOrder, ActionBusContext> Execute { get; internal set; }
    
        public ActionBusContext SubExecution { get; set; }
        public Exception Exception { get; internal set; }
    }

}