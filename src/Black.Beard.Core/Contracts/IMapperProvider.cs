using System;
using System.Collections.Generic;

namespace Bb.Mappings.Models
{
    public interface IMapperProvider
    {

        IMapper GetMapper(Type source, Type target);

        IEnumerable<(Type, Func<IMapper>)> GetMappers(Type source);

    }
}