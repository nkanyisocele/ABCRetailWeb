using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ABCRetail.Functions
{
    public class BlobStorageFunction
    {
        [Function("UploadBlobMedia")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("product-images");
            await containerClient.CreateIfNotExistsAsync();

            string blobName = $"{Guid.NewGuid()}.jpg";
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(req.Body, new BlobHttpHeaders { ContentType = "image/jpeg" });

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(blobName);
            return response;
        }
    }
}


