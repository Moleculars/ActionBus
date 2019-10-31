using Bb.ReminderStore.Sgbd;

namespace Bb.ActionBus.Builders.Reminders
{
    public class ReminderStore : ReminderStoreSqlServer
    {

        public ReminderStore(ReminderConfiguration configuration)
            : base(configuration.ConnectionString, ActionBusBuilderConstants.SqlproviderInvariantName, configuration.WakeUpIntervalSeconds)
        {

        }


    }

}
// ReminderConfiguration