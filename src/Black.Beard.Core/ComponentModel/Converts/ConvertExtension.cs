using Bb.ComponentModel.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bb.ComponentModel.Converts
{

    [ExposeClass(Context =Constants.Cast)]
    public static class ConvertExtension
    {

        public static Guid ToGuid(string self)
        {
            return Guid.Parse(self);
        }


    }

}
