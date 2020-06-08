using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFunctionCode1
{
    public static class Wait
    {
        [FunctionName("Wait")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("Wait_Hello", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("Wait_Hello", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("Wait_Hello", "London"));
            outputs.Add(await context.CallActivityAsync<string>("Wait_Hello", "Trouble"));
            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("Wait_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello Not You!! {name}!";
        }

        [FunctionName("Wait_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Wait", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
        [FunctionName("StatusCheck")]
        public static async Task<IActionResult> StatusCheck(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestMessage req,
        [DurableClient] IDurableClient client,
        ILogger log)
        {
            var runtimeStatus = new List<OrchestrationRuntimeStatus>();

            runtimeStatus.Add(OrchestrationRuntimeStatus.Pending);
            runtimeStatus.Add(OrchestrationRuntimeStatus.Running);

            var status = await client.GetStatusAsync(new DateTime(2015, 10, 10), null, runtimeStatus);
            return (ActionResult)new OkObjectResult(new Status() { HasRunning = (status.Count != 0) });
        }
    }

    internal class Status
    {
        public Status()
        {
        }

        public bool HasRunning { get; set; }
    }
}