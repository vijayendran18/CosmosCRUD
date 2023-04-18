using System.Diagnostics;
using System.Net;
using CRUD.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CRUD
{
    public class MigrateSourceToDestination
    {
        private readonly ILogger _logger;
        private readonly CosmosDbClient _srcClient, _destClient;

        public MigrateSourceToDestination(ILoggerFactory loggerFactory, SourceCosmosClient srcContainer, DestinationCosmosClient destContainer)
        {
            _logger = loggerFactory.CreateLogger<MigrateSourceToDestination>();
            _srcClient = srcContainer;
            _destClient = destContainer;
        }

        [Function("MigrateSourceToDestination")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            var t = new Stopwatch();
            t.Start();

            // Query the source container for all documents
            var query = new QueryDefinition("SELECT * FROM c");
            int maxItemCount = 100;
            var queryRequestOptions = new QueryRequestOptions()
            {
                //PartitionKey = new Microsoft.Azure.Cosmos.PartitionKey("id"),
                MaxItemCount = maxItemCount, // Maximum number of items to retrieve per query
                MaxBufferedItemCount = 5000, // Maximum number of items to buffer before writing to destination container
            };

            var queryIterator = _srcClient._container.GetItemQueryIterator<ToDoItem>(query, requestOptions: queryRequestOptions);

            int counts = 0;

            // Iterate through the query results and insert each document into the destination container
            while (queryIterator.HasMoreResults)
            {
                var queryResult = await queryIterator.ReadNextAsync();
                counts += queryResult.Count;

                foreach (ToDoItem item in queryResult)
                {
                    try
                    {
                        await _destClient._container.UpsertItemAsync(item);
                        //await _srcClient._container.UpsertItemAsync<ToDoItem>(document, partitionKey: new PartitionKey(document.id.ToString()));
                    }
                    catch (CosmosException ex)
                    {
                        // Handle Cosmos DB exception
                        Console.WriteLine($"CosmosException: [ItemID : {item.id}] - {ex.StatusCode} - {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        // Handle general exception
                        Console.WriteLine($"Exception: [ItemID : {item.id}] - {ex.Message}");
                    }

                }
            }

            t.Stop();
            var timeElapsed = t.Elapsed;

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString($"{counts} items migrated in {timeElapsed}");
            return response;
        }
    }
}
