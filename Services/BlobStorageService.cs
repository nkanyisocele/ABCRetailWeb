using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace ABCRetailWeb.Services
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private const string ContainerName = "product-images";

       
        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        // Uploads an image stream directly to your Azure Blob Container
        public async Task<string> UploadBlobAsync(Stream fileStream, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.None);

            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, overwrite: true);

            // Returns the raw public URL string of the image
            return blobClient.Uri.ToString();
        }

        // Retrieves all images from your Azure container to render on your screen
        public async Task<List<string>> GetAllBlobsAsync()
        {
            var blobUrls = new List<string>();
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

            if (await containerClient.ExistsAsync())
            {
                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    var blobClient = containerClient.GetBlobClient(blobItem.Name);

                    // Generate a secure SAS token valid for 2 hours
                    var sasBuilder = new BlobSasBuilder
                    {
                        BlobContainerName = ContainerName,
                        BlobName = blobItem.Name,
                        Resource = "b", // Represents a single blob
                        ExpiresOn = DateTimeOffset.UtcNow.AddHours(2)
                    };
                    sasBuilder.SetPermissions(BlobSasPermissions.Read);

                    // Append the token securely to the image URI path
                    var sasUri = blobClient.GenerateSasUri(sasBuilder);
                    blobUrls.Add(sasUri.ToString());
                }
            }
            return blobUrls;
        }
    }
}
