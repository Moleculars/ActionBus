using Bb.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bb.ActionBus
{
    public class ActionRepositories
    {

        public ActionRepositories()
        {
            _dic = new Dictionary<string, ActionModel>();
        }

        public ActionRepositories Register<T>(Func<T> ctor, int countInstance)
        {
            var r = new ActionRepository<T>(ctor, TypeDiscovery.Instance);
            r.Initialize(_dic, countInstance);
            return this;
        }

        public bool Execute(ActionOrder order)
        {

            if (!_dic.TryGetValue(order.Name, out ActionModel action))
                throw new InvalidOperationException(order.Name);

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

            order.ExecuteStarted = DateTimeOffset.Now;

            bool ok = false;
            try
            {
                order.Result = action.Execute(_arg.ToArray());
                ok = true;
            }
            catch (Exception e)
            {
                order.Result = e;
            }

            order.ExecuteStoped = DateTimeOffset.Now;

            return ok;

        }


        public IEnumerable<ActionModelDesciptor> GetMethods()
        {
            foreach (var item in _dic)
                yield return item.Value.Sign();
        }


        private readonly Dictionary<string, ActionModel> _dic;

    }




}
