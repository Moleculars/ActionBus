using System;
using System.Collections.Generic;
using System.Text;

namespace Bb
{

    public static class ClockActionBus
    {

        static ClockActionBus()
        {
            ClockActionBus._span = TimeSpan.Zero;
        }

        public static Func<DateTimeOffset> Now = () => DateTimeOffset.Now.Add(_span);
        private static TimeSpan _span;

        public static void Add(TimeSpan span)
        {
            _span = _span.Add(span);
        }

        public static void Reset()
        {
            ClockActionBus._span = TimeSpan.Zero;
        }

    }

}
