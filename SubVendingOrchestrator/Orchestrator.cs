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

            var nodes = TaskList
            .Select(item => {
                var obj = (JObject)item;
                var key = obj.Properties().First().Name;
                var dependsOn = (string)obj[key]?["dependsOn"];
                return new { Key = key, DependsOn = dependsOn, Node = obj };
            })
            .ToList();

            // Build the dependency graph
            var graph = nodes.ToDictionary(n => n.Key, n => new List<string>());
            var inDegree = nodes.ToDictionary(n => n.Key, n => 0);
            var taskKeys = new HashSet<string>(nodes.Select(n => n.Key)); // Track existing task keys

            foreach (var node in nodes)
            {
                if (!string.IsNullOrEmpty(node.DependsOn) && taskKeys.Contains(node.DependsOn))
                {
                    graph[node.DependsOn].Add(node.Key);
                    inDegree[node.Key]++;
                }
                else if (!string.IsNullOrEmpty(node.DependsOn) && !taskKeys.Contains(node.DependsOn))
                {
                    // Log or handle ignored dependencies
                    Console.WriteLine($"Ignoring non-existent dependency '{node.DependsOn}' for task '{node.Key}'");
                }
            }

            // Kahn's Algorithm: Process nodes with in-degree of 0
            var queue = new Queue<string>(inDegree.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key));
            var sortedOrder = new List<string>();

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                sortedOrder.Add(current);

                foreach (var neighbor in graph[current])
                {
                    inDegree[neighbor]--;
                    if (inDegree[neighbor] == 0)
                        queue.Enqueue(neighbor);
                }
            }

            // Output the tasks in sorted order
            foreach (var key in sortedOrder)
            {
                Console.WriteLine($"Processing: {key}");
                var node = nodes.First(n => n.Key == key).Node;
                Console.WriteLine(node.ToString());
                Console.WriteLine(new string('-', 30));
            }

        }
    }

}
