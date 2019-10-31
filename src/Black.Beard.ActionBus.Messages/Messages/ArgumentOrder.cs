using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Bb.ActionBus
{

    public class ArgumentOrder
    {

        static ArgumentOrder()
        {
            ArgumentOrder.dic = new Dictionary<Type, Func<string, object>>();

            dic.Add(typeof(string), (value) => value);
            dic.Add(typeof(short), (value) => Convert.ToInt16(value));
            dic.Add(typeof(int), (value) => Convert.ToInt32(value));
            dic.Add(typeof(long), (value) => Convert.ToInt64(value));
            dic.Add(typeof(DateTime), (value) => Convert.ToDateTime(value));
            dic.Add(typeof(bool), (value) => Convert.ToBoolean(value));
            dic.Add(typeof(byte), (value) => Convert.ToByte(value));
            dic.Add(typeof(char), (value) => Convert.ToChar(value));
            dic.Add(typeof(decimal), (value) => Convert.ToDecimal(value));
            dic.Add(typeof(double), (value) => Convert.ToDouble(value));
            dic.Add(typeof(ushort), (value) => Convert.ToUInt16(value));
            dic.Add(typeof(uint), (value) => Convert.ToUInt32(value));
            dic.Add(typeof(ulong), (value) => Convert.ToUInt64(value));
            dic.Add(typeof(sbyte), (value) => Convert.ToSByte(value));
            dic.Add(typeof(Single), (value) => Convert.ToSingle(value));
            dic.Add(typeof(Guid), (value) => Guid.Parse(value));

            dic.Add(typeof(short?), (value) => (short?)Convert.ToInt16(value));
            dic.Add(typeof(int?), (value) => (int?)Convert.ToInt32(value));
            dic.Add(typeof(long?), (value) => (long?)Convert.ToInt64(value));
            dic.Add(typeof(DateTime?), (value) => (DateTime?)Convert.ToDateTime(value));
            dic.Add(typeof(bool?), (value) => (bool?)Convert.ToBoolean(value));
            dic.Add(typeof(byte?), (value) => (byte?)Convert.ToByte(value));
            dic.Add(typeof(char?), (value) => (char?)Convert.ToChar(value));
            dic.Add(typeof(decimal?), (value) => (decimal?)Convert.ToDecimal(value));
            dic.Add(typeof(double?), (value) => (double?)(object)Convert.ToDouble(value));
            dic.Add(typeof(ushort?), (value) => (ushort?)(object)Convert.ToUInt16(value));
            dic.Add(typeof(uint?), (value) => (uint?)(object)Convert.ToUInt32(value));
            dic.Add(typeof(ulong?), (value) => (ulong?)(object)Convert.ToUInt64(value));
            dic.Add(typeof(sbyte?), (value) => (sbyte?)(object)Convert.ToSByte(value));
            dic.Add(typeof(Single?), (value) => (Single?)(object)Convert.ToSingle(value));
            dic.Add(typeof(Guid?), (value) => (Guid?)Guid.Parse(value));

        }

        public string Value { get; set; }

        public T Unserialize<T>()
        {

            if (ArgumentOrder.dic.TryGetValue(typeof(T), out Func<string, object> f))
                return (T)f(Value);

            var result = JsonConvert.DeserializeObject<T>(Value, _jsonDeserializationSettings);

            return result;

        }

        private static readonly JsonSerializerSettings _jsonDeserializationSettings = new JsonSerializerSettings
        {

        };
        private static readonly Dictionary<Type, Func<string, object>> dic;
    }

}