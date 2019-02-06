using Bb.ComponentModel.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Bb.ComponentModel
{

    public class MethodDiscoveryAssembly : IMethodDiscovery
    {

        public MethodDiscoveryAssembly(ITypeReferential typeReferential, string startWith, Type inheritFrom)
        {
            _typeReferential = typeReferential;
            _startWith = startWith;
            _inheritFrom = inheritFrom;
        }

        /// <summary>
        /// Return list of method for the specified arguments
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="returnType"></param>
        /// <param name="methodSign"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">returnType</exception>
        /// <exception cref="ArgumentNullException">methodSign</exception>
        public virtual IEnumerable<BusinessAction<T>> GetActions<T>(BindingFlags bindings, Type returnType, List<Type> methodSign) //where T : Context
        {

            this.methodSign = methodSign;
            this.returnType = returnType;

            Type[] types = GetTypes();
            var actions = GetActions_Impl<T>(bindings, GetTypes());

            if (!string.IsNullOrEmpty(_startWith))
                return actions.Where(c => c.RuleName.StartsWith(_startWith)).ToList();

            return actions;

        }

        public virtual Type[] GetTypes()
        {
            var types = _typeReferential.GetTypesWithAttributes(typeof(ExposeClassAttribute));

            if (_inheritFrom != null)
                return types.Where(c => c.IsAssignableFrom(_inheritFrom)).ToArray();

            return types.ToArray();

        }

        /// <summary>
        /// Permet de retourner la liste des methodes d'evaluation disponibles dans les types fournis.
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        private IEnumerable<BusinessAction<T>> GetActions_Impl<T>(BindingFlags bindings, params Type[] types)
        {

            var _result = new List<BusinessAction<T>>();

            foreach (var type in types)
            {

                var items = MethodDiscovery.GetMethods(type, bindings, returnType, methodSign);

                foreach (var method in items)
                {

                    RegisterMethodAttribute attribute = Attribute.GetCustomAttribute(method, typeof(RegisterMethodAttribute)) as RegisterMethodAttribute;
                    if (attribute != null)
                        _result.Add(new BusinessAction<T>
                        {
                            Method = method,
                            Type = type,
                            RuleName = attribute.DisplayName,
                            Origin = $"Assembly {type.AssemblyQualifiedName}",
                        });
                }
            }

            return _result;
        }

        private Type returnType;
        private List<Type> methodSign;
        private readonly ITypeReferential _typeReferential;
        private readonly string _startWith;
        private readonly Type _inheritFrom;
    }
}
