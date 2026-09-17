using Azure.Storage.Files.Shares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ABCRetail.Functions
{
    public class FileShareFunction
    {
        private readonly IConfiguration _configuration;

        public FileShareFunction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("WriteLogToFile")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            string connectionString = _configuration.GetValue<string>("AzureWebJobsStorage");
            var shareClient = new ShareClient(connectionString, "app-logs");
            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient("application-trace-log.txt");

            string incomingLog = await new StreamReader(req.Body).ReadToEndAsync();
            string logLine = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {incomingLog}\n";
            byte[] logBytes = Encoding.UTF8.GetBytes(logLine);

            if (await fileClient.ExistsAsync())
            {
                var properties = await fileClient.GetPropertiesAsync();
                long currentLength = properties.Value.ContentLength;

                var openOptions = new Azure.Storage.Files.Shares.Models.ShareFileOpenWriteOptions { MaxSize = currentLength + logBytes.Length };
                using var uploadStream = await fileClient.OpenWriteAsync(true, currentLength, openOptions);
                await uploadStream.WriteAsync(logBytes, 0, logBytes.Length);
            }
            else
            {
                await fileClient.CreateAsync(logBytes.Length);
                using var uploadStream = await fileClient.OpenWriteAsync(false, 0);
                await uploadStream.WriteAsync(logBytes, 0, logBytes.Length);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            return response;
        }
    }
}


