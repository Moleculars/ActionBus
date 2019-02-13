using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Bb.ActionBus
{

    public class ActionOrder
    {

        public ActionOrder()
        {
            this.Arguments = new List<ArgumentOrder>();
        }

        public Guid Uuid { get; set; }

        public DateTimeOffset PushedAt { get; set; }

        public string Name { get; set; }

        public List<ArgumentOrder> Arguments { get; set; }

        [JsonIgnore]
        public DateTimeOffset ExecutedAt { get; set; }

        [JsonIgnore]
        public object Result { get; set; }

        public override string ToString()
        {
            string stringMessage = JsonConvert.SerializeObject(this, _jsonSerializationSettings);
            return stringMessage;
        }

        public static ActionOrder Unserialize(string message)
        {
            return JsonConvert.DeserializeObject<ActionOrder>(message, _jsonSerializationSettings);
        }
        
        private static readonly JsonSerializerSettings _jsonSerializationSettings = new JsonSerializerSettings
        {

            Formatting = System.Diagnostics.Debugger.IsAttached
                            ? Formatting.Indented
                            : Formatting.None,

            NullValueHandling = NullValueHandling.Ignore,

        };

    }

}
