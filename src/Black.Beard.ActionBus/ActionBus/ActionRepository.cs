using Bb.ComponentModel;
using Bb.ComponentModel.Attributes;
using Bb.ComponentModel.Factories;
using Bb.Expresssions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Bb.ActionBus
{

    internal abstract class ActionRepository
    {

        internal abstract void Initialize(Dictionary<string, ActionModel> dic);

    }

    internal class ActionRepository<TType, TContext> : ActionRepository
        where TType : class
        where TContext : ActionBusContext, new()
    {

        static ActionRepository()
        {
            _getValue = typeof(ActionOrder).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Instance);

        }

        internal ActionRepository(ITypeReferential typeReferential)
        {

            _types = typeReferential;

            var attribute = TypeDescriptor.GetAttributes(typeof(TType)).OfType<ExposeClassAttribute>().FirstOrDefault();
            if (attribute != null)
                _rootName = attribute.Name ?? typeof(TType).Name;
            else
                _rootName = GetType().Name;

        }

        internal override void Initialize(Dictionary<string, ActionModel> dic)
        {

            var provider = new ActionMethodDiscovery(typeof(TType)) { Context = ActionOrder.BusinessActionBusContants };
            var actions = provider.GetActions(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, null)
                .ToList()
                ;

            Trace.WriteLine($"Register custom type '{typeof(TType)}' -> '{_rootName}'");

            foreach (var item in actions)
            {
                var parameters = item.Item4.GetParameters();
                if (parameters.Length > 0 && typeof(TContext).IsAssignableFrom(parameters[0].ParameterType))
                {
                    var name = _rootName + "." + item.Item3.DisplayName;
                    var act = new ActionModel<TType, TContext>((item.Item4.Attributes & MethodAttributes.Static) == MethodAttributes.Static)
                    {
                        Type = item.Item1,
                        Name = name,
                        Delegate = CreateDelegate(item, name),
                        Parameters = parameters.Select(c => new KeyValuePair<string, Type>(c.Name, c.ParameterType)).ToList(),

                    };

                    Trace.WriteLine($"Register custom action '{act.Name}' -> {item.Item4.ToString()}");

                    dic.Add(act.Name, act);
                }

            }

        }

        private static Action<TType, TContext, ActionOrder> CreateDelegate((Type, ExposeClassAttribute, ExposeMethodAttribute, MethodInfo) item, string name)
        {

            SourceCodeMethod code = new SourceCodeMethod();

            var args = item.Item4.GetParameters();

            var argument1 = code.AddParameter(typeof(TType), "instance");
            var argument2 = code.AddParameter(typeof(TContext), "ctx");
            var argument3 = code.AddParameter(typeof(ActionOrder), "order");

            var _resultVariable = code.AddVar(typeof(object), "_result");

            code.Assign(_resultVariable, ExpressionHelper.AsConstant(null));

            List<Expression> variables = new List<Expression>();
            for (int i = 0; i < args.Length; i++)
            {
                var parameter = args[i];

                if (parameter.ParameterType == typeof(TContext))
                    variables.Add(argument2);
                
                else
                {
                    var m = _getValue.MakeGenericMethod(parameter.ParameterType);
                    var u = code.AddVar(parameter.ParameterType, "_" + parameter.Name);
                    code.Assign(u, argument3.Call(m, parameter.Name.AsConstant()));
                    variables.Add(u);
                }

            }

            GetembeddedCallAction(code, item, argument1, argument2, variables.ToArray(), name); // Copied in local for understand

            var finaleMethod = code.GenerateLambda<Action<TType, TContext, ActionOrder>>();

            var _delegate = finaleMethod.Compile();

            return _delegate;
        }



        #region PackageMethod (copied of Black.beard.Core.BusinessAction<TContext> for understand what doing the packaging)

        /// <summary>
        /// Return an expression from the method
        /// </summary>
        /// <param name="argumentContext"></param>
        /// <returns></returns>
        public static MethodCallExpression GetCallMethod(MethodInfo method, Expression instance, Expression[] arguments)
        {

            var m = instance == null    // Business method
                ? method.Call(arguments)
                : instance.Call(method, arguments)
                ;

            return m;

        }

        /// <summary>
        /// Return an expression from the method
        /// </summary>
        /// <param name="argumentContext"></param>
        /// <returns></returns>
        public static void GetembeddedCallAction(
            SourceCodeMethod code,
            (Type, ExposeClassAttribute, ExposeMethodAttribute, MethodInfo) item,
            Expression instance,
            ParameterExpression ctx, 
            Expression[] arguments,
            string name
            )
        {

            var property = typeof(TContext).GetProperty("Result");

            Expression m = GetCallMethod(item.Item4, instance, arguments);

            var _try = new SourceCode();

            if (item.Item4.ReturnType != typeof(void))
            {
                m = ctx.Property(property).AssignFrom(m);
                _try.Add(m)
                    .Add(GetCallLogAction(item.Item4, name, ctx.Property(property), ctx, arguments))
                    ;
            }
            else
            {
                _try.Add(m)
                    .Add(GetCallLogAction(item.Item4, name, ExpressionHelper.AsConstant(null), ctx, arguments))
                    ;
            }

            var catchBlk = new SourceCode()
            {

            };

            var p1 = catchBlk.AddVar(typeof(Exception), "exception");

            code.Try
                (

                    _try,

                    new CatchStatement()
                    {
                        Parameter = p1,
                        Body = catchBlk
                            .Add(GetCallLogActionException(item.Item4, name, p1, ctx, arguments))
                            .ReThrow()
                    }

                );
        
        }


        /// <summary>
        /// Return an expression from the method
        /// </summary>
        /// <param name="argumentContext"></param>
        /// <returns></returns>
        public static Expression GetCallLogAction(MethodInfo methodLog, string rulename, Expression result, ParameterExpression _context, params Expression[] arguments)
        {

            // build custom method
            List<Expression> _args = new List<Expression>(arguments.Length);
            var parameters = methodLog.GetParameters()
                .ToArray();

            // Build log method
            List<Expression> _argValues = new List<Expression>();
            List<Expression> _argNames = new List<Expression>();

            for (int i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                _argNames.Add(Expression.Constant(parameters[i].Name));
                _argValues.Add(argument.ConvertIfDifferent(typeof(object)));
            }

            var call = BusinessLog<TContext>.MethodLogResult.Call(
                rulename.AsConstant(),
                result,
                _context,
                typeof(string).NewArray(_argNames),
                typeof(object).NewArray(_argValues)
            );

            return call;

        }

        /// <summary>
        /// Return an expression from the method
        /// </summary>
        /// <param name="argumentContext"></param>
        /// <returns></returns>
        public static Expression GetCallLogActionException(MethodInfo method, string rulename, ParameterExpression parameter, ParameterExpression _context, params Expression[] arguments)
        {

            // build custom method
            List<Expression> _args = new List<Expression>(arguments.Length);
            var parameters = method.GetParameters()
                .ToArray();

            // Build log method
            List<Expression> _argValues = new List<Expression>();
            List<Expression> _argNames = new List<Expression>();

            for (int i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                _argNames.Add(Expression.Constant(parameters[i].Name));
                _argValues.Add(argument.ConvertIfDifferent(typeof(object)));
            }

            var call = BusinessLog<TContext>.MethodLogResultException.Call(
                rulename.AsConstant(),
                parameter,
                _context,
                typeof(string).NewArray(_argNames),
                typeof(object).NewArray(_argValues)
            );

            return call;

        }


        #endregion

        private static readonly MethodInfo _getValue;
        private readonly ITypeReferential _types;
        private readonly string _rootName;

    }


}
