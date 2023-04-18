using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Identity;
using CRUD.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Azure.Core;
using Microsoft.Azure.Documents;
using Microsoft.WindowsAzure.Storage.Queue;
using CRUD.Rest;

namespace CRUD
{
    public class ClientCheck
    {
        private readonly ILogger _logger;
        private readonly SourceCosmosConfiguration _config;
        private readonly CosmosDbClient _srcClient;

        public ClientCheck(ILoggerFactory loggerFactory, SourceCosmosConfiguration config, SourceCosmosClient srcClient)
        {
            _logger = loggerFactory.CreateLogger<CreateItems>();
            _config = config;
            _srcClient = srcClient;
        }

        [Function("ClientCheck")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {

            //// Construct the connection string with SAS token
            ////string accountEndpointUrl = _config.Endpoint;
            //string databaseName = _config.Database;
            //string containerName = _config.Container;
            ////string sasToken = "<your-sas-token>";

            ////string connectionSring = $"AccountEndpoint={accountEndpointUrl};Database={databaseName};Container={containerName};SharedAccessSignature={sasToken}";

            ////// Create a CosmosClient object
            ////var cosmosClient = new CosmosClient(connectionSring);
            ////---------------------------------------------------------------------------------------------------------------------------------------------

            //string cosmosDbEndpoint = _config.Endpoint;
            //string clientId = "9bc22870-5bdb-498e-a6f3-a65e97545cd8";
            //var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = clientId });
            ////var credential1 = new DefaultAzureCredential(new DefaultAzureCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzureChina });
            //CosmosClient client = new CosmosClient(cosmosDbEndpoint, credential);


            //var database = client.GetDatabase(databaseName);
            //var container = database.GetContainer(containerName);
            //var query = "select * from c";

            //var queryIterator = container.GetItemQueryIterator<ToDoItem>(query);
            //int counts = 0;
            //// Iterate through the query results and insert each document into the destination container
            //while (queryIterator.HasMoreResults)
            //{
            //    var queryResult = await queryIterator.ReadNextAsync();
            //    counts += queryResult.Count;
            //}


            var api = new CosmosAPI();
            var data = await api.getDocumentsAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            response.WriteString(data);
            return response;
        }




    }
}
