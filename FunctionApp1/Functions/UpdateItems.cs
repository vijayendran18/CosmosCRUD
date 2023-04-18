using System.Diagnostics;
using System.Net;
using CRUD.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CRUD
{
    public class UpdateItems
    {
        private readonly ILogger _logger;
        private readonly CosmosDbClient _srcClient;

        public UpdateItems(ILoggerFactory loggerFactory, SourceCosmosClient srcContainer)
        {
            _logger = loggerFactory.CreateLogger<UpdateItems>();
            _srcClient = srcContainer;
        }

        [Function("UpdateItems")]
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
                        item.currencyCode = "INR";
                        var responseStatus = await _srcClient._container.UpsertItemAsync(item);
                        //await _srcClient._container.UpsertItemAsync<ToDoItem>(document, partitionKey: new PartitionKey(document.id.ToString()));

                        if (responseStatus.StatusCode == System.Net.HttpStatusCode.Created)
                        {
                            //Console.WriteLine("Item updated successfully");
                        }
                        else
                        {
                            Console.WriteLine($"Item updation failed with status code: {responseStatus.StatusCode}");
                        }

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
            response.WriteString($"{counts} items updated in {timeElapsed}");
            return response;
        }
    }
}
