using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Microsoft.Azure.Cosmos;

namespace Company.Function
{
    public static class GetResumeCounter
    {
        [FunctionName("GetResumeCounter")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            CosmosClient cosmosClient = new(
                            connectionString: Environment.GetEnvironmentVariable("AzureResumeConnectionString")!
                            );
            
            Database db = cosmosClient.GetDatabase("AzureResume");

            Container container = db.GetContainer("Counter");

            PartitionKey partitionKey = new("1");

            Counter counter = await container.ReadItemAsync<Counter>("404f9278-331f-489c-a16c-8b31e9e61141",partitionKey);

            counter.count += 1;
            
            await container.UpsertItemAsync<Counter>(counter);

            var jsonToReturn = JsonConvert.SerializeObject(counter);

            return new HttpResponseMessage(System.Net.HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn,Encoding.UTF8,"application/Json")
            };
        }
    }
}