using Bb.ComponentModel;
using Bb.ComponentModel.Attributes;
using Bb.ComponentModel.Factories;
using Bb.Core.Pools;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Bb.ActionBus
{

    internal abstract class ActionRepository
    {

        internal abstract void Initialize(Dictionary<string, ActionModel> dic, int countInstance);

    }

    internal class ActionRepository<T> : ActionRepository
        where T : class
    {

        internal ActionRepository(ITypeReferential typeReferential, IConfiguration configuration)
        {

            _types = typeReferential;
            _configuration = configuration;

            Factory<T> factory = null;

            if (configuration != null)
            {
                factory = new Factory<T>(typeof(IConfiguration)); // ?? new Factory<T>();
                if (!factory.IsEmpty)
                    _ctor = () => factory.Create(_configuration);
            }

            if (factory == null || factory.IsEmpty)
            {
                factory = new Factory<T>();
                if (!factory.IsEmpty)
                    _ctor = () => factory.Create();
                else
                    Trace.WriteLine($"failed to create ctor factory for {typeof(T)}", TraceLevel.Error.ToString());

            }

            if (Attribute.GetCustomAttribute(_ctor().GetType(), typeof(ExposeClassAttribute)) is ExposeClassAttribute attribute)
                _rootName = attribute.DisplayName;
            else
                _rootName = GetType().Name;

        }

        internal override void Initialize(Dictionary<string, ActionModel> dic, int countInstance)
        {

            var provider = new ActionMethodDiscovery(typeof(T));
            var actions = provider.GetActions<object>(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null, null)
                .Where(c => c.Context == "BusinessAction")
                .ToList()
                ;

            Trace.WriteLine($"Register custom type '{typeof(T)}' -> '{_rootName}'");

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

                Trace.WriteLine($"Register custom action '{act.Name}' -> {item.Method.ToString()}");

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
        private readonly IConfiguration _configuration;
        private readonly Func<T> _ctor;
        private readonly string _rootName;

    }

}
