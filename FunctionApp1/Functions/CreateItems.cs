using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using CRUD.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CRUD
{
    public class CreateItems
    {
        private readonly ILogger _logger;
        private readonly CosmosDbClient _srcClient;

        public CreateItems(ILoggerFactory loggerFactory, SourceCosmosClient srcContainer)
        {
            _logger = loggerFactory.CreateLogger<CreateItems>();
            _srcClient = srcContainer;
        }

        [Function("CreateItems")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            
            //await _srcClient._cosmosClient.CreateDatabaseAsync("Test");

            ToDoItem item = new ToDoItem();
            int counts = 100;
            var t = new Stopwatch();
            t.Start();

            for (int i = 0; i < counts; i++)
            {
                try
                {
                    item = new ToDoItem() { id = Guid.NewGuid(), creationTime = DateTime.Now };
                    var responseStatus = await _srcClient._container.CreateItemAsync<ToDoItem>(item);

                    if (responseStatus.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        //Console.WriteLine("Item created successfully");
                        Console.WriteLine("\n" + i);
                    }
                    else
                    {
                        Console.WriteLine($"Item creation failed with status code: {responseStatus.StatusCode}");
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

            t.Stop();
            var timeElapsed = t.Elapsed;

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString($"{counts} items created in {timeElapsed}");
            return response;
        }




    }
}
