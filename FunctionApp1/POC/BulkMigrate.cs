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
    public class BulkMigrate
    {
        private readonly ILogger _logger;
        private readonly CosmosDbClient _srcClient, _destClient;

        public BulkMigrate(ILoggerFactory loggerFactory, SourceCosmosClient srcContainer, DestinationCosmosClient destContainer)
        {
            _logger = loggerFactory.CreateLogger<MigrateSourceToDestination>();
            _srcClient = srcContainer;
            _destClient = destContainer;
        }

        [Function("BulkMigrate")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            var t = new Stopwatch();
            t.Start();
            int counts = 0;

            await _destClient._database.DefineContainer(_destClient._container.Id, "/id")
                    .WithIndexingPolicy()
                        .WithIndexingMode(IndexingMode.Consistent)
                        .WithIncludedPaths()
                            .Attach()
                        .WithExcludedPaths()
                            .Path("/*")
                            .Attach()
                    .Attach()
                .CreateAsync(50000);



            //HttpClient client = new HttpClient();
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("master", "");
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Add("x-ms-documentdb-is-upsert", "true");
            //client.DefaultRequestHeaders.Add("x-ms-version", "2021-03-15");





            t.Stop();
            var timeElapsed = t.Elapsed;

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString($"{counts} items migrated in {timeElapsed}");
            return response;
        }


        //private static IReadOnlyCollection<ToDoItem> GetItemsToInsert()
        //{
        //    return new Bogus.Faker<ToDoItem>()
        //    .StrictMode(true)
        //    //Generate item
        //    .RuleFor(o => o.id, f => Guid.NewGuid().ToString()) //id
        //    .RuleFor(o => o.username, f => f.Internet.UserName())
        //    .RuleFor(o => o.pk, (f, o) => o.id) //partitionkey
        //    .Generate(AmountToInsert);
        //}



    }
}
