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

        public static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs(object obj, string prefix = "")
        {
            var type = obj.GetType();
            var genericEnumerableInterface = type.GetInterfaces().FirstOrDefault(
                i => i.IsGenericType &&
                     i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            var isIterable = genericEnumerableInterface != null && !(obj is string);
            
            foreach (var kvp in isIterable ? ConvertEnumerable((IEnumerable)obj, prefix) : ConvertObject(obj, prefix))
                yield return kvp;
        }

        private static IEnumerable<KeyValuePair<string, string>> ConvertEnumerable(IEnumerable obj, string prefix)
        {
            var index = 0;
            foreach (var item in obj)
            {
                var itemType = item.GetType();
                if (!itemType.IsPrimitive && !(item is string))
                {
                    var pre = string.IsNullOrWhiteSpace(prefix)
                        ? $"{index}"
                        : $"{prefix}[{index}]";
                    foreach (var kvp in ConvertObject(item, pre))
                        yield return kvp;
                }
                else
                {
                    yield return new KeyValuePair<string, string>($"{prefix}[{index}]", $"{item}");
                }

                index++;
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> ConvertObject(object obj, string prefix = "")
        {
            var type = obj.GetType();
            if (type.IsPrimitive || type.IsEnum || obj is string)
                yield return new KeyValuePair<string, string>(prefix, $"{obj}");
            else
                foreach (var prop in type.GetProperties())
                {
                    var propValue = prop.GetValue(obj);
                    var subPrefix = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                    foreach (var kvp in ToKeyValuePairs(propValue, subPrefix))
                        yield return kvp;
                }
        }
    }
}
