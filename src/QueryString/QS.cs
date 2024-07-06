using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace QueryString
{
    public class QS
    {
        public static string Stringify(object obj, string prefix = "")
        {
            return string.Join("&", ToKeyValuePairs(obj, prefix).Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }

        private static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs(object obj, string prefix = "")
        {
            var type = obj.GetType();
            var genericEnumerableInterface = type.GetInterfaces().FirstOrDefault(
                i => i.IsGenericType &&
                     i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            var isIterable = genericEnumerableInterface != null && !(obj is string);
            if (isIterable)
            {
                var i = 0;
                foreach (var item in (IEnumerable)obj)
                {
                    var itemType = item.GetType();
                    if (!itemType.IsPrimitive && !(item is string))
                    {
                        var properties = itemType.GetProperties();
                        foreach (var prop in properties)
                        {
                            var propValue = prop.GetValue(item);
                            var subPrefix = string.IsNullOrEmpty(prefix)
                                ? $"{i}[{prop.Name}]"
                                : $"{prefix}[{i}][{prop.Name}]";
                            foreach (var kvp in ToKeyValuePairs(propValue, subPrefix))
                                yield return kvp;
                        }
                    }
                    else
                    {
                        yield return new KeyValuePair<string, string>($"{prefix}[{i}]", $"{item}");
                    }

                    i++;
                }
            }
            else if (type.IsPrimitive || type.IsEnum || obj is string)
            {
                yield return new KeyValuePair<string, string>(prefix, $"{obj}");
            }
            else
            {
                var properties = type.GetProperties();
                foreach (var prop in properties)
                {
                    var propValue = prop.GetValue(obj);
                    var subPrefix = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                    foreach (var kvp in ToKeyValuePairs(propValue, subPrefix))
                        yield return kvp;
                }
            }
        }
    }
}
