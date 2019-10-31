using Bb.ComponentModel;
using Bb.ComponentModel.Attributes;
using System;

namespace Bb.ActionBus
{
    [ExposeClass(Context = ConstantsCore.Cast)]
    public static class ConvertExtension
    {

        public static Guid ToGuid(string self)
        {
            return Guid.Parse(self);
        }


    }


}
