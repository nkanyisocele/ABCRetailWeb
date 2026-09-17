using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Data.Tables;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace ABCRetail.Functions
{
    public class TableStorageFunction
    {
        private readonly IConfiguration _configuration;

        public TableStorageFunction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("StoreTableData")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
           
            // 1. Read the JSON string into a modern, standard TableEntity object
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var customer = JsonConvert.DeserializeObject<TableEntity>(requestBody);

            // 2. Set up the connection using your local settings string
            string connectionString = _configuration.GetValue<string>("AzureWebJobsStorage");
            var tableClient = new TableClient(connectionString, "CustomerProfiles");
            await tableClient.CreateIfNotExistsAsync();

            // 3. Set the keys needed for Azure Table Storage
            customer.PartitionKey = "RetailCustomers";
            customer.RowKey = Guid.NewGuid().ToString();

            // 4. Save the object directly into the database table
            await tableClient.UpsertEntityAsync(customer);


            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Table record successfully written.");
            return response;
        }
    }

    public class DynamicTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public Azure.ETag ETag { get; set; }
    }
}


