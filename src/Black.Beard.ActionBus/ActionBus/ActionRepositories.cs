using Bb.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Bb.ActionBus
{

    public abstract class ActionRepositories // : IDisposable 
    {

        public ActionRepositories()
        {
            _dic = new Dictionary<string, ActionModel>();

        }

        public abstract object ExecuteOnObject(ActionOrder order);

        internal IServiceProvider ServiceProvider { get => _serviceProvider; }

        protected IServiceProvider _serviceProvider;
        protected readonly Dictionary<string, ActionModel> _dic;


    }

#warning ne pas oublier d'implémenter idisposable pour se désabonner des queues propremment et vider le travail correctement
    public class ActionRepositories<TContext> : ActionRepositories
    where TContext : ActionBusContext, new()
    {

        public ActionRepositories()
        {
        }

        public ActionRepositories<TContext> Inject(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
            return this;
        }

        #region Injection of the ServiceProvider

        public ActionRepositories<TContext> Register(Type type)
        {

            // resolve the generic method use to register action. (call the next method 'public ActionRepositories Register<T>() where T : class') 
            var genericMethod = GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(c => c.Name == "Register" && c.IsGenericMethod)
                .MakeGenericMethod(type);

            genericMethod.Invoke(this, new object[] { });

            return this;
        }

        /// <summary>
        /// Registers all methods used to business method.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <returns></returns>
        public ActionRepositories<TContext> Register<TType>() where TType : class
        {
            var r = new ActionRepository<TType, TContext>(TypeDiscovery.Instance);
            r.Initialize(_dic);
            return this;
        }

        #endregion Injection of the ServiceProvider


        public override object ExecuteOnObject(ActionOrder order)
        {
            return this.Execute(order);
        }


        /// <summary>
        /// Push a request in right custom output
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public TContext Execute(ActionOrder order)
        {

            TContext context = null;

            if (!_dic.TryGetValue(order.Name, out ActionModel action))
                throw new InvalidOperationException(order.Name);

            order.ExecutedAt = ClockActionBus.Now();

            Stopwatch sp = new Stopwatch();
            try
            {
                sp.Start();
                context = (TContext)action.Execute(this, order);
                order.Result = context.Result;
                order.Success = true;
            }
            catch (Exception e)
            {
                context.Exception = e;
            }
            finally
            {
                sp.Stop();
                string key = string.Empty;
                Trace.WriteLine(new { Key = key, action.Name, ElapsedTime = sp.Elapsed }, ConstantsCore.PerfMon);
            }

            return context;

        }

        public IEnumerable<ActionModelDesciptor> GetMethods()
        {
            foreach (var item in _dic)
                yield return item.Value.Sign();
        }

    }

}
