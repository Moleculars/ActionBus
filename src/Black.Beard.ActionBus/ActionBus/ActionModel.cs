using Bb.Core.Helpers;
using Bb.Core.Pools;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Bb.ActionBus
{
    internal abstract class ActionModel
    {

        public string Name { get; internal set; }

        public string Origin { get; internal set; }

        public Type Type { get; internal set; }

        public abstract object Execute(string[] arguments);

        public List<KeyValuePair<string, Type>> Parameters { get; internal set; }

        internal ActionModelDesciptor Sign()
        {
            return new ActionModelDesciptor() { Name = Name, Arguments = Parameters };
        }

    }

    internal class ActionModel<T> : ActionModel
    {

        private ObjectPool<T> _pool;

        public ActionModel(ObjectPool<T> pool)
        {
            _pool = pool;
        }

        public Func<T, string[], object> Delegate { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Execute(string[] arguments)
        {
            using (var instance = _pool.WaitForGet())
            {
                object result = Delegate(instance.Instance, arguments);
                return result;
            }
        }

    }

}
