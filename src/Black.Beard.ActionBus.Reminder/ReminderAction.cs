using Bb.ComponentModel.Attributes;
using Bb.Reminder;
using System;

namespace Bb.ActionBus.Reminder
{

    [ExposeClass("Reminder", Context = ActionBusContants.BusinessActionBus)]
    public class ReminderAction
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="ReminderAction"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public ReminderAction(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _service = _serviceProvider.GetService(typeof(IReminderRequest)) as IReminderRequest;
        }


        /// <summary>
        /// Remind message for x minutes specified by argument delayInMinute. and push the message at specified address
        /// </summary>
        /// <param name="uuid">The UUID. fonctionnal call identifier</param>
        /// <param name="binding">The binding. the way to push message at end</param>
        /// <param name="address">The address to push the message after the delay</param>
        /// <param name="message">The message to push</param>
        /// <param name="delayInMinute">The delay in minute before push message.</param>
        /// <param name="headers">The headers. additional argument if the way accept this specifiecation, like http or broker.</param>
        /// <returns></returns>
        [ExposeMethod("TechnicalWatch", Context = ActionBusContants.BusinessActionBus)]
        public bool PushParent(Guid uuid, string binding, string address, string message, int delayInMinute, string headers)
        {

            var model = new WakeUpRequestModel()
            {
                Uuid = uuid,
                Binding = binding,
                Address = address,
                Message = message,
                DelayInMinute = delayInMinute,
                Headers = headers
            };

            _service.Watch(model);

            return true;

        }

        [ExposeMethod("Cancel", Context = ActionBusContants.BusinessActionBus)]
        public bool CancelPushParent(Guid uuid)
        {
            _service.Cancel(uuid);
            return true;
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly IReminderRequest _service;

    }
}
