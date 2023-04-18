using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Formatting;
using System.Threading;
using System.Security.Cryptography;
using System.Net;

namespace CRUD.Rest
{

    public class CosmosAPI
    {
        static readonly string endpoint = "https://privatecosmos-v.documents.azure.com:443/";
        static readonly string masterKey = "VrZgDtmxizyy6CwLWv5bwJw3LtqkZEdcWCq5HoFHpaivM9NxBnAsDk9YQNikDtb27SvH5EgEaZanACDb2f3RWw==";
        static readonly Uri baseUri = new Uri(endpoint);
        static readonly string databaseId = "ToDoList";
        static readonly string collectionId = "sourcePoc";
        static readonly string documentId = "test";
        static bool idBased = true;

        static readonly string utc_date = DateTime.UtcNow.ToString("r");


        public async Task<string> getDocumentsAsync()
        {
            var method = HttpMethod.Get;
            var resourceType = ResourceType.docs;
            var resourceLink = $"dbs/{databaseId}/colls/{collectionId}";
            var requestDateString = DateTime.UtcNow.ToString("r");
            var auth = GenerateMasterKeyAuthorizationSignature(method, resourceType, resourceLink, requestDateString, masterKey);

            var httpClient = new System.Net.Http.HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("authorization", auth);
            httpClient.DefaultRequestHeaders.Add("x-ms-date", requestDateString);
            httpClient.DefaultRequestHeaders.Add("x-ms-version", "2018-12-31");
            //httpClient.DefaultRequestHeaders.Add("x-ms-documentdb-partitionkey", $"[\"{partitionKey}\"]");

            var requestUri = new Uri($"{endpoint}/{resourceLink}/docs");
            var httpRequest = new HttpRequestMessage { Method = method, RequestUri = requestUri };

            var httpResponse = await httpClient.SendAsync(httpRequest);
            var data = httpResponse.Content.ReadAsStringAsync();
            return data.Result;
        }


        string GenerateMasterKeyAuthorizationSignature(HttpMethod verb, ResourceType resourceType, string resourceLink, string date, string key)
        {
            var keyType = "master";
            var tokenVersion = "1.0";
            var payload = $"{verb.ToString().ToLowerInvariant()}\n{resourceType.ToString().ToLowerInvariant()}\n{resourceLink}\n{date.ToLowerInvariant()}\n\n";

            var hmacSha256 = new System.Security.Cryptography.HMACSHA256 { Key = Convert.FromBase64String(key) };
            var hashPayload = hmacSha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
            var signature = Convert.ToBase64String(hashPayload);
            var authSet = WebUtility.UrlEncode($"type={keyType}&ver={tokenVersion}&sig={signature}");

            return authSet;
        }

       
    }

    enum ResourceType
    {
        dbs,
        colls,
        docs,
        sprocs,
        pkranges,
    }

}