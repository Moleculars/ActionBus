using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Bb.ActionBus
{

    public class ActionOrderConverter : JsonConverter<ActionOrder>
    {

        public override ActionOrder ReadJson(JsonReader reader, Type objectType, ActionOrder existingValue, bool hasExistingValue, JsonSerializer serializer)
        {

            JObject jObject = JObject.Load(reader);

            var result = new ActionOrder()
            {
            };

            foreach (var item in jObject)
            {
                switch (item.Key)
                {

                    case "Uuid":
                        result.Uuid = Guid.Parse(item.Value.Value<string>());
                        break;

                    case "ExecutedAt":
                        result.ExecutedAt = item.Value.Value<DateTime>();
                        break;

                    case "Name":
                        result.Name = item.Value.Value<string>();
                        break;

                    case "PushedAt":
                        result.PushedAt = item.Value.Value<DateTime>();
                        break;

                    case "Result":
                        result.Result = item.Value.ToString();
                        break;

                    case "Success":
                        result.Success = Convert.ToBoolean(item.Value.ToString());
                        break;

                    case "Arguments":
                        var a = (JObject)item.Value;
                        foreach (var item2 in a.Properties())
                            result.Argument(item2.Name, item2.Value.ToString());
                        break;

                    default:
                        break;

                }
            }

            return result;

        }

        public override void WriteJson(JsonWriter writer, ActionOrder value, JsonSerializer serializer)
        {

            var arguments = new JObject();

            JObject o = new JObject
            {
                new JProperty(nameof(value.Uuid), new JValue(value.Uuid)),
                new JProperty(nameof(value.ExecutedAt), new JValue(value.ExecutedAt)),
                new JProperty(nameof(value.Name), new JValue(value.Name)),
                new JProperty(nameof(value.PushedAt), new JValue(value.PushedAt)),
                new JProperty(nameof(value.Result), new JValue(value.Result)),
                new JProperty(nameof(value.Success), new JValue(value.Success)),
                new JProperty(nameof(value.Arguments), arguments),
            };

            foreach (var item in value.Arguments)
            {
                var v = item.Value.Value;
                if ((v.StartsWith("{") && v.EndsWith("}")) || (v.StartsWith("[") && v.EndsWith("]")))
                    arguments.Add(new JProperty(item.Key, JObject.Parse(v)));
                else
                    arguments.Add(new JProperty(item.Key, new JValue(v)));
            }

            o.WriteTo(writer);

        }

    }

}
