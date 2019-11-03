using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Bb.ActionBus
{

    public class ActionOrder
    {

        static ActionOrder()
        {

            var formatting = System.Diagnostics.Debugger.IsAttached
            ? Formatting.Indented
            : Formatting.None;

            _jsonSerializationSettings = new JsonSerializerSettings
            {
                Formatting = formatting,
                NullValueHandling = NullValueHandling.Ignore,

            };
            _jsonSerializationSettings.Converters.Add(new ActionOrderConverter());
        }

        public ActionOrder()
        {
        }

        public string Uuid { get; set; }

        public string Name { get; set; }

        public DateTimeOffset PushedAt { get; set; }

        public DateTimeOffset ExecutedAt { get; set; }

        public object Result { get; set; }

        public bool Success { get; set; }


        public Dictionary<string, ArgumentOrder> Arguments { get; set; } = new Dictionary<string, ArgumentOrder>();

        public ActionOrder Argument(string key, string value)
        {
            this.Arguments.Add(key, new ArgumentOrder() { Value = value });
            return this;
        }

        public T GetValue<T>(string name)
        {

            if (Arguments.TryGetValue(name, out ArgumentOrder a))
                return a.Unserialize<T>();

            return default;

        }

        public override string ToString()
        {
            string stringMessage = JsonConvert.SerializeObject(this, _jsonSerializationSettings);
            return stringMessage;
        }

        public static ActionOrder Unserialize(string message)
        {
            return JsonConvert.DeserializeObject<ActionOrder>(message, _jsonSerializationSettings);
        }

        private static readonly JsonSerializerSettings _jsonSerializationSettings;

        public static string BusinessActionBusContants => Bb.ActionBus.ActionBusContants.BusinessActionBus;

    }

}
