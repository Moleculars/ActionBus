using Bb.ComponentModel;
using Bb.ComponentModel.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
