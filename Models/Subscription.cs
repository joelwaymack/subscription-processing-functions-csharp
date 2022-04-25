using System;
using Newtonsoft.Json;

namespace Company.Function.Models;

public class Subscription
{
    [JsonProperty("id")]
    public Guid Id { get; set; }
    [JsonProperty("customerId")]
    public string customerId { get; set; }
    [JsonProperty("level")]
    public SubscriptionLevel Level { get; set; }
    [JsonProperty("createdTimestamp")]
    public DateTime CreatedTimestamp { get; set; }
    [JsonProperty("isActive")]
    public bool IsActive { get; set; } = true;
}