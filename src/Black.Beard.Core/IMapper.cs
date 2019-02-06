using System;

namespace Bb
{

    public interface IMapper
    {


        Type Source { get; }

        Type Target { get; }

        object Map(object source, object target);

    }
}