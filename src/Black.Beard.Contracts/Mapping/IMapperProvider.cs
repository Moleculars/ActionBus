using System;
using System.Collections.Generic;

namespace Bb.Mapping
{
    public interface IMapperProvider
    {

        IMapper GetMapper(Type source, Type target);

        IEnumerable<(Type, Func<IMapper>)> GetMappers(Type source);

    }
}