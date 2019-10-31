using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Bb.ActionBus
{

    public abstract class ActionModel
    {

        public string Name { get; internal set; }

        public Type Type { get; internal set; }

        public abstract object Execute(ActionRepositories parent, ActionOrder actionOrder);

        public List<KeyValuePair<string, Type>> Parameters { get; internal set; }

        public bool IsStatic { get; internal set; }

        internal ActionModelDesciptor Sign()
        {
            return new ActionModelDesciptor() { Name = Name, Arguments = Parameters };
        }

    }

    internal class ActionModel<TType, TContext> : ActionModel
        where TContext : ActionBusContext, new()
    {

        public ActionModel(bool _static)
        {
            this.IsStatic = _static;
        }

        public Action<TType, TContext, ActionOrder> Delegate { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Execute(ActionRepositories parent, ActionOrder actionOrder)
        {

            TType instance = default;


            if (!this.IsStatic)
            {
                IServiceProvider serviceProvider = parent.ServiceProvider;
                instance = (TType)serviceProvider.GetService(typeof(TType));
            }

            Func<ActionOrder, ActionBusContext> execute = (order) => (TContext)parent.ExecuteOnObject(order);

            var ctx = new TContext()
            {
                Order = actionOrder,
                Execute = execute,
            };

            Delegate(instance, ctx, actionOrder);

            return ctx;

        }

    }

}
