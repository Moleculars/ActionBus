using System;
using System.Collections.Generic;
using System.Text;

namespace Bb.ActionBus
{
    
    public class ActionRunner<TContext> 
        where TContext : ActionBusContext, new()
    {


        public ActionRunner(ActionRepositories<TContext> repository)
        {
            this._repository = repository;
        }

        public TContext Evaluate(string payload)
        {
            var order = ActionOrder.Unserialize(payload);
            var result = this._repository.ExecuteOnObject(order);
            return (TContext)result;
        }


        private readonly ActionRepositories<TContext> _repository;

    }

}
