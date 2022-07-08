using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Company.Function.Models;
using System.Collections.Generic;
using System.Linq;

namespace Company.Function.Handlers;
public static class PaymentApiHandler
{
    [FunctionName("GetSubscriptionPayments")]
    public static IActionResult GetSubscriptionPayments(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "customers/{customerId}/subscriptions/{subscriptionId:guid}/payments")] HttpRequest req,
        [CosmosDB("%DatabaseName%",
            "%PaymentCollection%",
            ConnectionStringSetting = "CosmosDBConnection",
            SqlQuery = "SELECT * FROM p where p.customerId = {customerId} AND p.subscriptionId = {subscriptionId}")] IEnumerable<Payment> payments,
        Guid subscriptionId,
        string customerId,
        ILogger log)
    {
        log.LogInformation($"{payments.Count()} payments retrieved for subscription {subscriptionId} for customer {customerId}");

        return new OkObjectResult(payments);
    }

    [FunctionName("GetCustomerPayments")]
    public static IActionResult GetCustomerPayments(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "customers/{customerId}/payments")] HttpRequest req,
        [CosmosDB("%DatabaseName%",
            "%PaymentCollection%",
            ConnectionStringSetting = "CosmosDBConnection",
            SqlQuery = "SELECT * FROM p where p.customerId = {customerId}")] IEnumerable<Payment> payments,
        string customerId,
        ILogger log)
    {
        log.LogInformation($"{payments.Count()} payments retrieved for customer {customerId}");

        return new OkObjectResult(payments);
    }
}
