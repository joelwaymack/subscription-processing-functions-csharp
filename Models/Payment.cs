using System;
using Newtonsoft.Json;

namespace Company.Function.Models;

public class Payment
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("customerId")]
    public string CustomerId { get; set; }

    [JsonProperty("subscriptionId")]
    public Guid SubscriptionId { get; set; }

    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("createdTimestamp")]
    public DateTime CreatedTimestamp { get; set; }
}