using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Company.Function.Models;

[JsonConverter(typeof(StringEnumConverter))]
public enum SubscriptionLevel
{
    Basic,
    Standard,
    Premium
}