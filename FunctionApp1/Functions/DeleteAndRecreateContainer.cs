using System.Net;
using CRUD.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CRUD
{
    public class DeleteAndRecreateContainer
    {
        private readonly ILogger _logger;
        private readonly CosmosDbClient _srcClient;

        public DeleteAndRecreateContainer(ILoggerFactory loggerFactory, SourceCosmosClient srcContainer)
        {
            _logger = loggerFactory.CreateLogger<DeleteAndRecreateContainer>();
            _srcClient = srcContainer;
        }

        [Function("DeleteAndRecreateContainer")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            await _srcClient._container.DeleteContainerAsync();
            await _srcClient._database.CreateContainerAsync(_srcClient._container.Id, "/id");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString("Cleared!");
            return response;
        }
    }
}
