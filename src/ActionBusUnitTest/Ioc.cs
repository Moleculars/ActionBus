using System;
using System.Collections.Generic;

namespace ActionBusUnitTest
{
    public class Ioc : System.IServiceProvider
    {

        public Ioc Add(Type type, Func<object> f)
        {
            _instances.Add(type, f);
            return this;
        }

        public object GetService(Type serviceType)
        {

            if (_instances.TryGetValue(serviceType, out Func<object> f))
                return f();

            throw new NotImplementedException();
        }

        private Dictionary<Type, Func<object>> _instances = new Dictionary<Type, Func<object>>();

    }

}
