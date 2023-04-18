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
    public class RestCall
    {
        private readonly ILogger _logger;
        private readonly SourceCosmosConfiguration _config;
        private readonly CosmosDbClient _srcClient;

        public RestCall(ILoggerFactory loggerFactory, SourceCosmosConfiguration config, SourceCosmosClient srcClient)
        {
            _logger = loggerFactory.CreateLogger<CreateItems>();
            _config = config;
            _srcClient = srcClient;
        }

        [Function("RestCall")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {

            var api = new CosmosAPI();
            var data = await api.getDocumentsAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            response.WriteString(data);
            return response;
        }




    }
}
