using Bb.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Bb.ActionBus
{

#warning ne pas oublier d'implémenter idisposable pour se désabonner des queues propremment et vider le travail correctement
    public class ActionRepositories // : IDisposable 
    {

        public ActionRepositories(IConfiguration configuration, IServiceCollection services, EventHandler<ActionOrderEventArgs> acquitmentQueue, EventHandler<ActionOrderEventArgs> deadQueue, int countInstance)
        {

            _countInstance = countInstance;
            _services = services;

            if (acquitmentQueue != null)
                AcquitmentQueue += acquitmentQueue;

            if (deadQueue != null)
                DeadQueue += deadQueue;

            _configuration = configuration;
            _dic = new Dictionary<string, ActionModel>();

        }

        public ActionRepositories Register(Type type)
        {

            var genericMethod = GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(c => c.Name == "Register" && c.IsGenericMethod)
                .MakeGenericMethod(type);

            genericMethod.Invoke(this, new object[] { });

            return this;
        }

        public ActionRepositories Register<T>() where T : class
        {
            var r = new ActionRepository<T>(TypeDiscovery.Instance, _configuration);
            r.Initialize(_dic, _countInstance);
            return this;
        }

        /// <summary>
        /// Push a request in right custom output
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool Execute(ActionOrder order, int tentatives)
        {

            if (!_dic.TryGetValue(order.Name, out ActionModel action))
                throw new InvalidOperationException(order.Name);

            order.ExecuteStarted = DateTimeOffset.Now;

            List<string> _arg = SortArguments(order, action);

            Stopwatch sp = new Stopwatch();

            bool ok = false;
            try
            {

                sp.Start();
                order.Result = action.Execute(_arg.ToArray());
                sp.Stop();
                order.ElapsedExecution = sp.Elapsed;

                sp.Restart();
                AcquitmentQueue?.Invoke(this, new ActionOrderEventArgs(order, _services, tentatives));
                sp.Stop();
                order.ElapsedAcquitment = sp.Elapsed;

                ok = true;
            }
            catch (Exception e)
            {
                order.Result = e;
                sp.Restart();
                DeadQueue?.Invoke(this, new ActionOrderEventArgs(order, _services, tentatives));
                sp.Stop();
                order.ElapsedAcquitment = sp.Elapsed;
            }

            order.ExecuteStoped = DateTimeOffset.Now;

            return ok;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<string> SortArguments(ActionOrder order, ActionModel action)
        {
            List<string> _arg = new List<string>();
            if (order.Arguments.Any(c => !string.IsNullOrEmpty(c.Name))) // If all name are specified we prefer match argument by name
            {
                var _arguments = order.Arguments.ToDictionary(c => c.Name);

                foreach (var item in action.Parameters)
                {
                    if (!_arguments.TryGetValue(item.Key, out ArgumentOrder arg))
                        throw new MissingMemberException(order.Name + "." + item.Key);
                    _arg.Add(arg.Value);
                }
            }
            else // select only values
                _arg.AddRange(order.Arguments.Select(c => c.Value));
            return _arg;
        }

        private event EventHandler<ActionOrderEventArgs> DeadQueue;
        private event EventHandler<ActionOrderEventArgs> AcquitmentQueue;

        public IEnumerable<ActionModelDesciptor> GetMethods()
        {
            foreach (var item in _dic)
                yield return item.Value.Sign();
        }

        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, ActionModel> _dic;

        public int _countInstance { get; }

        private readonly IServiceCollection _services;

    }

}
