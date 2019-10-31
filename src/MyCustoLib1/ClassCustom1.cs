using Bb.ComponentModel.Attributes;
using Microsoft.Extensions.Configuration;
using System;
using Bb.ActionBus;
using Bb.Brokers;

namespace MyCustoLib1
{

    // every methods exposed to action bus can't be void.

    [ExposeClass("business1", Context = ActionBusContants.BusinessActionBus)]
    public class ClassCustom1
    {

        public ClassCustom1(IServiceProvider serviceProvider)
        {

            _serviceProvider = serviceProvider;
            _configuration = serviceProvider.GetService(typeof(IConfiguration)) as IConfiguration;
            _brokers = serviceProvider.GetService(typeof(IFactoryBroker)) as IFactoryBroker;

            _publisherParent = _configuration.GetValue<string>("Parent");

        }


        #region PushMom

        [ExposeMethod("PushParent", Context = ActionBusContants.BusinessActionBus)]
        public bool PushParent(Guid id, string status, bool requiredMoreMessage)
        {

            var publisher = _brokers.CreatePublisher(_publisherParent);

            publisher.Publish(
                new
                {
                    Uuid = id,
                    status,
                    requiredMoreMessage,
                }
                , null
                );

            return true;

        }

        #endregion PushMom


        #region Message 2

        [ExposeMethod("CancelScan", Context = ActionBusContants.BusinessActionBus)]
        public bool CancelMessage(Guid uuid)
        {

            return true;

        }

        [ExposeMethod("PushScan", Context = ActionBusContants.BusinessActionBus)]
        public Guid PushMessage(string siteId, string scan)
        {

            return Guid.NewGuid();

        }

        #endregion Message 2

        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly IFactoryBroker _brokers;
        private readonly string _publisherParent;
    }

}
