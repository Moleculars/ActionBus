using Microsoft.Extensions.DependencyInjection;
using System;

namespace Bb.ActionBus
{
    public class ActionOrderEventArgs : EventArgs
    {

        public ActionOrderEventArgs(ActionOrder action, IServiceCollection services, int tentatives)
        {
            this.Action = action;
            this.Services = services;
            this.Tentatives = tentatives;
        }

        public ActionOrder Action { get; set; }

        public IServiceCollection Services { get; }

        public int Tentatives { get; }

    }

}
