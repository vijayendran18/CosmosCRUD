using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using System;
using System.Net.Http;

namespace CRUD.Models
{

    public class CosmosDbClient
    {
        public CosmosClient _cosmosClient { get; set; }
        public Database _database { get; set; }
        public Container _container { get; set; }

        public CosmosDbClient(CosmosConfiguration CosmosConfiguration)
        {

            try
            {

                CosmosClientOptions cosmosClientOptions = new CosmosClientOptions()
                {
                    ConnectionMode = ConnectionMode.Direct,
                    AllowBulkExecution = true,
                    //MaxRequestsPerTcpConnection = 1000,
                    //GatewayModeMaxConnectionLimit = 1024,
                    MaxRetryAttemptsOnRateLimitedRequests = 10,
                    MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),
                    //RequestTimeout = TimeSpan.FromSeconds(10),
                };

                //----------------Primary/secondary keys	
                this._cosmosClient = new CosmosClient(CosmosConfiguration.Endpoint, CosmosConfiguration.Key, cosmosClientOptions);

                //----------------Role-based access control
                //TokenCredential tokenCredential = new ClientSecretCredential(
                //    CosmosConfiguration.TenantId,
                //    CosmosConfiguration.ClientId,
                //    CosmosConfiguration.ClientSecret
                ////,new ClientSecretCredentialOptions() { }
                //);
                //this._cosmosClient = new CosmosClient(CosmosConfiguration.Endpoint, tokenCredential, cosmosClientOptions);

                //----------------Resource tokens
                //this._cosmosClient = new CosmosClient(CosmosConfiguration.Endpoint, authKeyOrResourceToken: CosmosConfiguration.ResourceToken);

                //-----------------BrokerWay

                //string token = getting from the API;
                //User user = null;
                //// Get the database
                ////Database database = cosmosClient.GetDatabase(CosmosConfiguration.Database);
                //this._cosmosClient = new CosmosClient(CosmosConfiguration.Endpoint, authKeyOrResourceToken: token);

                //-----------------User assigned managed identity
                // Create an instance of the CosmosClientOptions class.

                //var defaultAzureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions()
                //{
                //    ManagedIdentityClientId = "0c982b08-cc20-40d4-9823-f8eac82228b6",
                //    //AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                //    //ExcludeVisualStudioCredential = true,
                //    //ExcludeAzurePowerShellCredential = true,
                //});

                //this._cosmosClient = new CosmosClient(CosmosConfiguration.Endpoint, tokenCredential: defaultAzureCredential);

                //----------------------------------------------------------------------------------------------------------------

                this._database = _cosmosClient.GetDatabase(CosmosConfiguration.Database);
                this._container = _database.GetContainer(CosmosConfiguration.Container);

            }
            catch (CosmosException ex)
            {
                // Handle Cosmos DB exception
                Console.WriteLine($"CosmosException: {ex.StatusCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle general exception
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

    }


    public class SourceCosmosClient : CosmosDbClient
    {
        public SourceCosmosClient(CosmosConfiguration CosmosConfiguration) : base(CosmosConfiguration) { }
    }

    public class DestinationCosmosClient : CosmosDbClient
    {
        public DestinationCosmosClient(CosmosConfiguration CosmosConfiguration) : base(CosmosConfiguration) { }
    }



}
