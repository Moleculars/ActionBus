using Bb.ComponentModel;
using Bb.Core.Helpers;
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

        public ActionRepositories(int countInstance = 20)
        {
            _countInstance = countInstance;
            _dic = new Dictionary<string, ActionModel>();
        }

        public void Inject(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
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
            var r = new ActionRepository<T>(TypeDiscovery.Instance, _serviceProvider);
            r.Initialize(_dic, _countInstance);
            return this;
        }

        /// <summary>
        /// Push a request in right custom output
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool Execute(ActionOrder order)
        {

            if (!_dic.TryGetValue(order.Name, out ActionModel action))
                throw new InvalidOperationException(order.Name);

            order.ExecutedAt = Clock.GetNow;
            List<string> _arg = SortArguments(order, action);

            Stopwatch sp = new Stopwatch();
            bool ok = false;
            try
            {
                sp.Start();
                order.Result = action.Execute(_arg.ToArray());
                ok = true;
            }
            catch (Exception e)
            {
                order.Result = e;
            }
            finally
            {
                sp.Stop();
                string key = string.Empty;
                Trace.WriteLine(new { Key = key, action.Name, ElapsedTime = sp.Elapsed }, Constants.PerfMon);
            }

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

        public IEnumerable<ActionModelDesciptor> GetMethods()
        {
            foreach (var item in _dic)
                yield return item.Value.Sign();
        }

        private IServiceProvider _serviceProvider;
        private readonly Dictionary<string, ActionModel> _dic;

        public int _countInstance { get; }

    }

}
