using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace SubVendingOrchestrator
{
    public static class Orchestrator
    {
        [FunctionName("OrchestratorFunction")]
        public static async Task RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            // Get the JSON payload (dynamic JSON)
            var inputJson = context.GetInput<string>();
            JObject jsonObject = JObject.Parse(inputJson);

            // Get the JSON pipeline task definitions
            string taskDefinitionJson = await context.CallActivityAsync<string>("GetPipelineTaskDefinitions", null);
            JObject taskDefinitions = JObject.Parse(taskDefinitionJson);

            // Create an empty JArray
            JArray TaskList = new JArray();


            foreach (var property in jsonObject.Properties())
            {
                //Console.WriteLine($"Root Element: {property.Name}");
                JToken taskdetails = taskDefinitions.SelectToken($"$.{property.Name}");
                TaskList.Add(taskdetails);
            }

        }
    }

}
