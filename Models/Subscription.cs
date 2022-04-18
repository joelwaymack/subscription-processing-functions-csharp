using System;

namespace Company.Function.Models;

public class Subscription
{
    public Guid Id { get; set; }
    public string customerId { get; set; }
    public SubscriptionType Type { get; set; }
    public DateTime CreatedTimestamp { get; set; }
    public bool IsActive { get; set; } = true;
}