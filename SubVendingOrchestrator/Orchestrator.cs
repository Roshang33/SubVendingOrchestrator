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
using System.Text.Json.Nodes;

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

            // Step 1: Build dependency graph & metadata
            var graph = new Dictionary<string, List<string>>();
            var inDegree = new Dictionary<string, int>();
            var nodeMap = new Dictionary<string, JObject>();

            foreach (var item in TaskList)
            {
                var obj = (JObject)item;
                string key = obj.Properties().First().Name;  // Get the root key
                string dependsOn = obj[key]?["dependsOn"]?.ToString(); // Get dependency

                nodeMap[key] = obj; // Store object reference
                graph.TryAdd(key, new List<string>());  // Ensure key exists
                inDegree.TryAdd(key, 0);  // Ensure key exists

                if (!string.IsNullOrEmpty(dependsOn))
                {
                    graph.TryAdd(dependsOn, new List<string>());
                    graph[dependsOn].Add(key); // Add dependency edge
                    inDegree[key] = inDegree.GetValueOrDefault(key) + 1; // Increment dependency count
                }
            }

            // Step 2: Process nodes using Kahn's Algorithm (BFS)
            var queue = new Queue<string>(inDegree.Where(x => x.Value == 0).Select(x => x.Key));
            var sortedKeys = new List<string>();

            while (queue.TryDequeue(out string node))
            {
                sortedKeys.Add(node);
                foreach (var neighbor in graph[node])
                    if (--inDegree[neighbor] == 0) queue.Enqueue(neighbor);
            }

            // Step 3: Execute in topological order
            foreach (var key in sortedKeys)
            {
                Console.WriteLine($"Processing: {key}");
                Console.WriteLine(nodeMap[key].ToString());
                Console.WriteLine("----------------------");
            }

        }
    }

}
