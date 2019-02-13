using System;

namespace Bb.Mapping
{

    public interface IMapper
    {

        Type Source { get; }

        Type Target { get; }

        object Map(object source, object target);

    }
}