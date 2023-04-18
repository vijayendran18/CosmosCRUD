using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;


using System;
using Microsoft.Azure.Management.CosmosDB.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using CRUD.Models;
using Microsoft.Azure.Cosmos;
using Azure.Identity;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace CRUD.POC
{
    public class GetToken
    {
        private readonly ILogger _logger;
        private readonly SourceCosmosConfiguration _config;
        private readonly CosmosDbClient _srcClient;

        public GetToken(ILoggerFactory loggerFactory, SourceCosmosConfiguration config, SourceCosmosClient srcClient)
        {
            _logger = loggerFactory.CreateLogger<CreateItems>();
            _config = config;
            _srcClient = srcClient;
        }

        [Function("GetToken")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {

            string endpointUrl = _config.Endpoint;
            string primaryKey = _config.Key;
            string databaseName = _config.Database;
            string containerName = _config.Container;

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string permissionId = "contributer";
            string permissionMode = "all";

            var responceData = "";

            try
            {
                using (var client = new DocumentClient(new Uri(endpointUrl), primaryKey))
                {
                    var databaseUri = UriFactory.CreateDatabaseUri(databaseName);
                    var containerUri = UriFactory.CreateDocumentCollectionUri(databaseName, containerName);

                    // Create a permission object for the container
                    var permission = new Microsoft.Azure.Documents.Permission
                    {
                        Id = permissionId,
                        PermissionMode = permissionMode.ToLower() == "read" ? Microsoft.Azure.Documents.PermissionMode.Read : Microsoft.Azure.Documents.PermissionMode.All,
                        ResourceLink = containerUri.ToString()
                    };


                    // Add the permission to the database
                    permission = await client.CreatePermissionAsync(databaseUri, permission);

                    // Generate a resource token for the container
                    var feedOptions = new FeedOptions { PartitionKey = new Microsoft.Azure.Documents.PartitionKey(Undefined.Value) };
                    responceData = client.CreateDocumentQuery<Microsoft.Azure.Documents.Permission>(containerUri, feedOptions)
                                            .Where(p => p.Id == permissionId)
                                            .Select(p => p.Token)
                                            .AsEnumerable()
                                            .FirstOrDefault();

                }
            }
            catch (DocumentClientException ex)
            {
                responceData = ($"Error creating Cosmos DB permission: {ex.StatusCode} - {ex.Message}");
                _logger.LogError(responceData);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString(responceData);
            return response;
        }
    }
}
