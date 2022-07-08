using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Company.Function.Models;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Linq;
using Microsoft.Azure.Documents.Linq;

namespace Company.Function.Handlers;

public static class ProcessSubscriptionHandler
{
    [FunctionName("ProcessNewSubscription")]
    public static async Task ProcessNewSubscription(
        [CosmosDBTrigger(
            databaseName: "%DatabaseName%",
            collectionName: "%SubscriptionCollection%",
            ConnectionStringSetting = "CosmosDBConnection",
            LeaseCollectionName = "%SubscriptionLeaseCollection%",
            CreateLeaseCollectionIfNotExists = true)] IReadOnlyList<Document> subscriptionDocuments,
        [CosmosDB("%DatabaseName%",
            "%SubscriptionCollection%",
            ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<Subscription> subscriptionsOutput,
        [ServiceBus("%PaymentQueue%", Connection = "ServiceBusConnection")] IAsyncCollector<Subscription> paymentsOutput,
        ILogger log)
    {
        foreach (var subscriptionDocument in subscriptionDocuments)
        {
            var subscription = JsonConvert.DeserializeObject<Subscription>(subscriptionDocument.ToString());

            // Only process new subscriptions.
            if (!subscription.PaymentDay.HasValue)
            {
                log.LogInformation($"Subscription {subscription.Id} created for customer {subscription.CustomerId}");
                subscription.PaymentDay = subscription.CreatedTimestamp.Day;
                await Task.WhenAll(subscriptionsOutput.AddAsync(subscription), paymentsOutput.AddAsync(subscription));
            }
        }
    }

    [FunctionName("ProcessDailySubscriptions")]
    public static async Task ProcessDailySubscriptions(
        [TimerTrigger("0 0 0 * * *")] TimerInfo timerInfo,
        [CosmosDB("%DatabaseName%",
            "%SubscriptionCollection%",
            ConnectionStringSetting = "CosmosDBConnection")] DocumentClient client,
        [ServiceBus("%PaymentQueue%", Connection = "ServiceBusConnection")] IAsyncCollector<Subscription> paymentsOutput,
        ILogger log)
    {
        var databaseName = Environment.GetEnvironmentVariable("DatabaseName");
        var subscriptionCollection = Environment.GetEnvironmentVariable("SubscriptionCollection");

        var query = client.CreateDocumentQuery<Subscription>(
            UriFactory.CreateDocumentCollectionUri(databaseName, subscriptionCollection),
            new FeedOptions { EnableCrossPartitionQuery = true })
            .Where(s => s.IsActive)
            .AsQueryable();

        // Handle end-of-month processing.
        var subscriptionQuery = (DateTime.Today.Day == DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)
            ? query.Where(s => s.PaymentDay >= DateTime.Today.Day)
            : query.Where(s => s.PaymentDay == DateTime.Today.Day))
            .AsDocumentQuery();

        while (subscriptionQuery.HasMoreResults)
        {
            foreach (var subscriptionDocument in await subscriptionQuery.ExecuteNextAsync())
            {
                var subscription = JsonConvert.DeserializeObject<Subscription>(subscriptionDocument.ToString());
                log.LogInformation($"Subscription {subscription.Id} payment requested for customer {subscription.CustomerId}");
                await paymentsOutput.AddAsync(subscription);
            }
        }
    }
}
