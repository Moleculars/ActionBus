using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Bb.Core.ComponentModel
{

    /// <summary>

    /// object extension. convert all properies of the class in <see cref="Dictionary{string, object}"></see>/>
    /// </summary>
    public static class DictionnarySerializerExtension
    {


        
        /// <summary>
        /// format the serialization of the specified object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">object sour to serialized</param>
        /// <param name="format">output format</param>
        /// <returns>string of the format with the source propertie's serialized</returns>
        public static void SerializeObject(this Dictionary<string, object> self, StringBuilder stringBuilder, bool ignoreCase = true)
        {
            
            foreach (var item in self)
            {
                stringBuilder.Append(item.Key);
                stringBuilder.Append("=");
                stringBuilder.Append(item.Value.ToString());
                stringBuilder.Append(";");
            }

        }

        /// <summary>
        /// format the serialization of the specified object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">object sour to serialized</param>
        /// <param name="format">output format</param>
        /// <returns>string of the format with the source propertie's serialized</returns>
        public static Dictionary<string, object> GetDictionnaryProperties<T>(this T source, bool ignoreCase = true)
        {

            System.Diagnostics.Contracts.Contract.Requires(!object.Equals(source, null), "null reference exception 'source'");

            Type type = typeof(T).IsInterface
                    ? typeof(T)
                    : source.GetType();

            return GetPropertiesMethod<T>(type, ignoreCase)(source);

        }

        #region Compile object serialization

        /// <summary>
        /// Get compiled method or create
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private static Func<T, Dictionary<string, object>> GetPropertiesMethod<T>(Type type, bool ignoreCase)
        {

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Func<T, Dictionary<string, object>> result = null;

            if (Storage<T>.Get != null)
                result = Storage<T>.Get;

            else
                lock (Storage<T>._lock)
                    if (Storage<T>.Get != null)
                        result = Storage<T>.Get;
                    else
                        Storage<T>.Get = result = CompileObject<T>(type, ignoreCase);

            return result;

        }

        /// <summary>
        /// Compile method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeSource"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private static Func<T, Dictionary<string, object>> CompileObject<T>(Type typeSource, bool ignoreCase)
        {

            Type containerType = typeof(Dictionary<string, object>);
            Type stringType = typeof(string);
            var _properties = typeSource.GetAllProperties().Where(c => c.CanRead);
            var properties = new HashSet<string>();
            var m = containerType.GetNamedMethod("Add", typeof(string), typeof(object));
           
            // variables
            var parameter = Expression.Parameter(typeSource, "arg1");
            var dic = Expression.Variable(containerType, "dic");

            var lst = new List<Expression>()
            {
                // var dic = new Dictionary<string, object>();
                dic.CreateObject(),

            };

            foreach (PropertyInfo item in _properties)
            {
                var n = item.Name;

                if (ignoreCase)
                    n = n.ToLower();

                if (properties.Add(item.Name))
                {
                    var p = item.PropertyType;
                    var m1 = parameter.Member(item);
                    if (p == typeof(object))
                        lst.Add(dic.Invoke(m, Expression.Constant(n), m1));
                    else
                        lst.Add(dic.Invoke(m, Expression.Constant(n), m1.As(typeof(object))));
                }
            }
            // return builder.ToString();
            lst.Add(dic);

            // Create func
            BlockExpression block = Expression.Block(containerType, new ParameterExpression[] { dic }, lst);
            var lbd = Expression.Lambda<Func<T, Dictionary<string, object>>>(block, parameter);

            return lbd.Compile();

        }

        #endregion Compile object serialization

        #region private


        private class Storage<T>
        {
            public static Func<T, Dictionary<string, object>> Get { get; set; }
            public static volatile object _lock = new object();
        }

        #endregion private

    }

}
