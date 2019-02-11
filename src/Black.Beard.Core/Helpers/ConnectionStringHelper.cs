using Bb.ComponentModel.Accessors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Bb.Helpers
{

    public static class ConnectionStringHelper
    {

        public static string GenerateHelp(Type self)
        {

            var properties = PropertyAccessor.Get(self, true);

            StringBuilder sb = new StringBuilder(properties.Count() * 800);
            sb.AppendLine("");

            foreach (var property in properties)
            {

                if (!property.CanWrite)
                    continue;

                sb.AppendLine(property.Name);
                sb.AppendLine($"\ttype : {property.Type.Name}");

                if (property.Required)
                    sb.AppendLine("\tRequired");

                if (!string.IsNullOrEmpty(property.DisplayDescription))
                    sb.AppendLine($"\tdescription : {property.DisplayDescription}");

                if (property.DefaultValue != null)
                    sb.AppendLine($"\tdefault value : {property.DefaultValue}");

                if (property.Type.IsEnum)
                {
                    sb.Append("\t Restricted values (");
                    foreach (var item in Enum.GetNames(property.Type))
                        sb.Append($"{item}, ");
                    sb.Remove(sb.Length - 2, 2);
                    sb.AppendLine(")");
                }

            }

            return sb.ToString();

        }


        // Helper


        public static Dictionary<string, string> ToDictionary(string arguments)
        {

            Dictionary<string, string> result = new Dictionary<string, string>();

            var members = arguments.Split(';');

            foreach (var item in members)
            {

                var ii = item.Split('=');

                var key = ii[0];
                var valueText = string.Join('=', ii.Skip(1));

                if (valueText.StartsWith("@"))
                {
                    var newValueText = Environment.GetEnvironmentVariable(valueText.Substring(1));
                    if (!string.IsNullOrEmpty(newValueText))
                        valueText = newValueText;
                }
                else
                    Trace.WriteLine($"environment variable {valueText} not found", TraceLevel.Warning.ToString());

                result.Add(key, valueText);

            }

            return result;

        }


        public static bool Map<T>(T self, string arguments, bool respectCase = false)
        {

            var properties = PropertyAccessor.Get(self.GetType(), true);

            var members = arguments.Split(';');

            foreach (var item in members)
            {

                var ii = item.Split('=');

                var key = ii[0];
                var valueText = string.Join('=', ii.Skip(1));
                AccessorItem property = properties.Get(key, respectCase);
                if (property != null)
                {
                    if (valueText.StartsWith("@"))
                    {
                        var newValueText = Environment.GetEnvironmentVariable(valueText.Substring(1));
                        if (!string.IsNullOrEmpty(newValueText))
                            valueText = newValueText;
                    }
                    else
                        Trace.WriteLine($"environment variable {valueText} not found", TraceLevel.Warning.ToString());

                    object value = MyConverter.Unserialize(valueText, property.Type);

                    if (property != null)
                        property.SetValue(self, value);
                }

            }

            bool result = true;

            foreach (var item in properties)
            {
                if (item.Required && item.GetValue(self) == null)
                {
                    Trace.WriteLine($"{item.Name} is required");
                    result = false;
                }
            }

            return result;

        }

    }

}
