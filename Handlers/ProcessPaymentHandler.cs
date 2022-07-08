using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Company.Function.Models;
using System;

namespace Company.Function.Handlers;

public static class ProcessPaymentHandler
{
    [FunctionName("ProcessSubscriptionPayment")]
    public static async Task ProcessSubscriptionPayment(
        [ServiceBusTrigger("%PaymentQueue%", Connection = "ServiceBusConnection")] Subscription subscription,
        [CosmosDB("%DatabaseName%",
            "%PaymentCollection%",
            ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<Payment> paymentsOutput,
        ILogger log)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            CustomerId = subscription.CustomerId,
            SubscriptionId = subscription.Id,
            Amount = subscription.Level switch
            {
                SubscriptionLevel.Basic => 0.99m,
                SubscriptionLevel.Standard => 2.99m,
                SubscriptionLevel.Premium => 5.99m,
                _ => throw new ArgumentOutOfRangeException()
            },
            CreatedTimestamp = DateTime.UtcNow
        };

        // Fake payment processing here.

        await paymentsOutput.AddAsync(payment);
        log.LogInformation($"Subscription {subscription.Id} payment processed for customer {subscription.CustomerId}");
    }
}
