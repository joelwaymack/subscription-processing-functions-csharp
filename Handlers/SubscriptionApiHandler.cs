using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Company.Function.Models;
using System.Collections.Generic;
using System.Linq;

namespace Company.Function.Handlers;

public static class SubscriptionApiHandler
{
    [FunctionName("CreateSubscription")]
    public static async Task<IActionResult> CreateSubscription(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "customers/{customerId}/subscriptions")] HttpRequest req,
        [CosmosDB("%DatabaseName%",
            "%SubscriptionCollection%",
            ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<Subscription> subscriptionsOutput,
        string customerId,
        ILogger log)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var subscription = JsonConvert.DeserializeObject<Subscription>(requestBody);
        subscription.Id = Guid.NewGuid();
        subscription.CustomerId = customerId;
        subscription.CreatedTimestamp = DateTime.UtcNow;
        await subscriptionsOutput.AddAsync(subscription);

        log.LogInformation($"Subscription {subscription.Id} created for customer {subscription.CustomerId}");

        return new OkObjectResult(subscription);
    }

    [FunctionName("GetSubscriptions")]
    public static IActionResult GetSubscriptions(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "customers/{customerId}/subscriptions")] HttpRequest req,
        [CosmosDB("%DatabaseName%",
            "%SubscriptionCollection%",
            ConnectionStringSetting = "CosmosDBConnection",
            SqlQuery = "SELECT * FROM s where s.customerId = {customerId} AND s.isActive")] IEnumerable<Subscription> subscriptions,
        string customerId,
        ILogger log)
    {
        log.LogInformation($"{subscriptions.Count()} subscriptions retrieved for customer {customerId}");

        return new OkObjectResult(subscriptions);
    }

    [FunctionName("DeleteSubscription")]
    public static IActionResult DeleteSubscription(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "delete",
            Route = "customers/{customerId}/subscriptions/{subscriptionId:Guid}")] HttpRequest req,
        [CosmosDB("%DatabaseName%",
            "%SubscriptionCollection%",
            ConnectionStringSetting = "CosmosDBConnection",
            Id = "{subscriptionId}",
            PartitionKey = "{customerId}")] Subscription subscription,
        [CosmosDB("%DatabaseName%",
            "%SubscriptionCollection%",
            ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<Subscription> subscriptionsOutput,
        string customerId,
        Guid subscriptionId,
        ILogger log)
    {
        if (subscription == null)
        {
            return new NotFoundResult();
        }

        subscription.IsActive = false;
        subscriptionsOutput.AddAsync(subscription);

        log.LogInformation($"Subscription {subscriptionId} deleted for customer {customerId}");

        return new OkResult();
    }
}
