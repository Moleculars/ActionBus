using Bb.ComponentModel;
using Bb.ComponentModel.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bb.ActionBus.Builders.Reminders
{

    [ExposeClass(Context = ConstantsCore.Configuration, Name = "ReminderConfiguration", LifeCycle = IocScopeEnum.Singleton)]
    public class ReminderConfiguration
    {

        /// <summary>
        /// Gets or sets the publisher Name for republish.
        /// </summary>
        /// <value>
        /// The publisher name.
        /// </value>
        public string Republisher { get; set; }

        /// <summary>
        /// Gets or sets the connection string for connect reminder index.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        public string ConnectionString { get; set; }


        public int WakeUpIntervalSeconds { get; set; } = 20;

    }

}
