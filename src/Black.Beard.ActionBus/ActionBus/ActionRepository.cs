using Bb.ComponentModel;
using Bb.ComponentModel.Attributes;
using Bb.Core.Helpers;
using Bb.Core.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Bb.ActionBus
{

    internal class ActionRepository<T>
    {

        internal ActionRepository(Func<T> ctor, ITypeReferential typeReferential)
        {
            _types = typeReferential;
            _ctor = ctor;

            if (Attribute.GetCustomAttribute(ctor().GetType(), typeof(ExposeClassAttribute)) is ExposeClassAttribute attribute)
                _rootName = attribute.DisplayName;
            else
                _rootName = GetType().Name;

        }

        internal void Initialize(Dictionary<string, ActionModel> dic, int countInstance)
        {

            Type returnType = typeof(List<KeyValuePair<string, string>>);
            var provider = new ActionMethodDiscovery(typeof(T));
            var actions = provider.GetActions<object>(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, returnType, null);

            foreach (var item in actions)
            {

                var act = new ActionModel<T>(new ObjectPool<T>(_ctor, 10))
                {
                    Name = _rootName + "." + item.RuleName,
                    Delegate = CreateDelegate(item),
                    Parameters = item.Method.GetParameters().Select(c => new KeyValuePair<string, Type>(c.Name, c.ParameterType)).ToList(),
                    Origin = item.Origin,
                    Type = item.Type,
                };

                dic.Add(act.Name, act);

            }

        }

        private static Func<T, string[], object> CreateDelegate(BusinessAction<object> item)
        {

            var argument1 = Expression.Parameter(typeof(T), "instance");
            var variables = item.Method.GetParameters().Select(c => Expression.Variable(typeof(string), c.Name)).ToArray();

            var argument2 = Expression.Parameter(typeof(string[]), "arguments");

            var method = item.GetCallAction(argument1, argument2, variables);


            List<Expression> _instructions = new List<Expression>();
            for (int i = 0; i < variables.Length; i++)
                _instructions.Add(Expression.Assign(variables[i], Expression.ArrayIndex(argument2, Expression.Constant(i))));
            _instructions.Add(method.Item2);

            var returnTarget = Expression.Label(typeof(object));
            _instructions.Add(Expression.Return(returnTarget, method.Item1, typeof(object)));
            _instructions.Add(Expression.Label(returnTarget, Expression.Default(typeof(object))));

            var vars = variables.ToList();
            //vars.Add(argument2);
            vars.Add(method.Item1);
            var blk = Expression.Block(vars.ToArray(), _instructions.ToArray());
            var finaleMethod = Expression.Lambda<Func<T, string[], object>>(blk, new ParameterExpression[] { argument1, argument2 });

            var _delegate = finaleMethod.Compile();

            return _delegate;
        }

        private readonly ITypeReferential _types;
        private readonly Func<T> _ctor;
        private readonly string _rootName;
    }

    internal abstract class ActionModel
    {

        public string Name { get; internal set; }

        public string Origin { get; internal set; }

        public Type Type { get; internal set; }

        public abstract object Execute(string[] arguments);

        public List<KeyValuePair<string, Type>> Parameters { get; internal set; }

        internal ActionModelDesciptor Sign()
        {
            return new ActionModelDesciptor() { Name = Name, Arguments = this.Parameters };
        }

    }

    internal class ActionModel<T> : ActionModel
    {

        private ObjectPool<T> _pool;

        public ActionModel(ObjectPool<T> pool)
        {
            _pool = pool;
        }

        public Func<T, string[], object> Delegate { get; set; }

        public override object Execute(string[] arguments)
        {
            using (var instance = _pool.WaitForGet())
            {
                object result = Delegate(instance.Instance, arguments);
                return result;
            }
        }

    }




}
