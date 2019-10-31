using Bb.ComponentModel;
using System;

namespace Bb.ActionBus
{

    public class ActionMethodDiscovery : MethodDiscoveryAssembly
    {

        public ActionMethodDiscovery(Type type) : base(TypeDiscovery.Instance, null, null)
        {
            _type = type;
        }

        public override Type[] GetTypes()
        {
            return new Type[] { _type };
        }

        private readonly Type _type;

    }

}
