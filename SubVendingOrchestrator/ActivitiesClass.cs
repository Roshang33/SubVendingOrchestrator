using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace SubVendingOrchestrator
{
    internal class ActivitiesClass
    {
        [FunctionName("GetPipelineTaskDefinitions")]
        public static async Task<string> GetPipelineTaskDefinitions([ActivityTrigger] string input)
        {
            // Normally, this data would come from a storage account or database

            string taskDefinitions = File.ReadAllText("C:\\Users\\Roshan\\source\\repos\\SubVendingOrchestrator\\workflow.json");
            return await Task.FromResult(taskDefinitions);
        }

    }
}
