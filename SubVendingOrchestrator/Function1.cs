using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace SubVendingOrchestrator
{
    public static class SubVendingOrchestrator
    {
        [FunctionName("SubVendingOrchestratorFunction")]
        public static async Task Run(
            [QueueTrigger("subvendingmessages", Connection = "AzureWebJobsStorage")] string queueMessage,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            log.LogInformation($"Queue trigger received message: {queueMessage}");

            // Start the orchestrator function
            string instanceId = await starter.StartNewAsync("OrchestratorFunction", queueMessage);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
        }
    }
}