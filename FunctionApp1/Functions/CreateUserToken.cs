using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using CRUD.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CRUD
{
    public class CreateUser
    {
        private readonly ILogger _logger;
        private readonly CosmosDbClient _srcClient;

        public CreateUser(ILoggerFactory loggerFactory, SourceCosmosClient srcContainer)
        {
            _logger = loggerFactory.CreateLogger<CreateItems>();
            _srcClient = srcContainer;
        }

        [Function("CreateUser")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {

            string userName = "readWrite";
            string permissionId = $"{userName}-{_srcClient._container.Id}";

            User user = null;

            try
            {
                user = _srcClient._database.GetUser(userName);
                await user.GetPermission(permissionId).ReadAsync();
            }
            catch (Exception ex)
            {
                user = await _srcClient._database.UpsertUserAsync(userName);
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

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString(responceMsg);
            return response;
        }




    }
}
