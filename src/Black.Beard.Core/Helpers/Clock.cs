using System;

namespace Bb.Core.Helpers
{

    public class Clock
    {

        /// <summary>
        /// Gets the current date time.
        /// </summary>
        /// <value>
        /// The get now
        /// </value>
        public static DateTimeOffset GetNow => DateTimeOffset.Now.Add(Decale);

        public static TimeSpan Decale = new TimeSpan(0, 0, 0, 0, 0);

    }

}
