using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace ABCRetail.Functions
{
    public class TableStorageFunction
    {
        [Function("StoreTableData")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var customer = JsonConvert.DeserializeObject<DynamicTableEntity>(requestBody);

            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var tableClient = new TableClient(connectionString, "CustomerProfiles");
            await tableClient.CreateIfNotExistsAsync();

            customer.PartitionKey = "RetailCustomers";
            customer.RowKey = Guid.NewGuid().ToString();

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


