using System.Net;
using Azure.Core;
using Azure.Identity;
using CRUD.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Management.Graph.RBAC.Fluent.Models;
using Microsoft.Extensions.Logging;


namespace CRUD.POC
{
    public class GetResourceToken
    {
        private readonly ILogger _logger;
        private readonly SourceCosmosConfiguration _config;
        private readonly CosmosDbClient _srcClient;

        public GetResourceToken(ILoggerFactory loggerFactory, SourceCosmosConfiguration config, SourceCosmosClient srcClient)
        {
            _logger = loggerFactory.CreateLogger<CreateItems>();
            _config = config;
            _srcClient = srcClient;
        }

        [Function("GetResourceToken")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {

            // Define your Cosmos DB endpoint URI, AAD tenant ID, client ID, database name, and container name
            string endpointUri = _config.Endpoint;
            string aadTenantId = _config.TenantId;
            string clientId = _config.ClientId;
            string databaseName = _config.Database;
            string containerName = _config.Container;

            TokenCredential credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                TenantId = aadTenantId,
                ExcludeSharedTokenCacheCredential = true,
            });

            CosmosClientOptions options = new CosmosClientOptions
            {
                ConnectionMode = Microsoft.Azure.Cosmos.ConnectionMode.Direct,
                //ApplicationName = "CRUD",
                SerializerOptions = new CosmosSerializationOptions
                {
                    IgnoreNullValues = true
                }
            };

            CosmosClient client = new CosmosClient(endpointUri, credential, options);

            // Get a reference to your Cosmos database and container
            Database database = client.GetDatabase(databaseName);
            Container container = database.GetContainer(containerName);

            // Define the permission that you want to create
            PermissionMode permissionMode = PermissionMode.Read;
            PermissionProperties permissionProperties = new PermissionProperties(permissionMode.ToString(), permissionMode, container);
            User user = null;


            string userName = "vijay";
            string permissionId = $"{userName}-{container.Id}-readWriteDelete";

            try
            {
                user = database.GetUser(userName);
                await user.GetPermission(permissionId).ReadAsync();
            }
            catch (Exception ex)
            {
                user = await database.UpsertUserAsync(userName);
            }

            string responceMsg = string.Empty;

            if (user != null)
            {
                try
                {

                    var permissionProp = new PermissionProperties(
                            id: permissionId,
                            permissionMode: PermissionMode.All,
                            container: _srcClient._container
                        //resourcePartitionKey: new PartitionKey("id"),

                        );

                    var data = await user.CreatePermissionAsync(permissionProp);

                    var permission = await user.GetPermission(permissionId).ReadAsync();
                    var token = permission.Resource.Token;
                    responceMsg = $"user created. {token}";
                }
                catch (Exception ex)
                {
                    responceMsg = ex.Message;
                }
            }
            else
            {
                responceMsg = $"user not created";
            }


            //// Create the permission
            //Permission permission = await container.CreatePermissionAsync(permissionProperties);

            //// Retrieve the resource token from the permission
            //string resourceToken = permission.ReadAsync().Result;

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString("");
            return response;
        }
    }
}
