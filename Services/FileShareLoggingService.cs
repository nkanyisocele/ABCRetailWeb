using Azure.Storage.Files.Shares;
using System.Text;

namespace ABCRetailWeb.Services
{
    public class FileShareLoggingService
    {
        private readonly ShareDirectoryClient _directoryClient;
        private const string ShareName = "app-logs";

        public FileShareLoggingService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");

            // Connect to the shared file share root drive layer
            var shareClient = new ShareClient(connectionString, ShareName);
            shareClient.CreateIfNotExists();

            // Get or create a specific diagnostic logs root subdirectory path
            _directoryClient = shareClient.GetRootDirectoryClient();
        }

        // Writes application system event text data straight into an isolated cloud text log file
        public async Task WriteLogAsync(string logFileName, string logMessage)
        {
            var fileClient = _directoryClient.GetFileClient(logFileName);

            string fullLogLine = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] {logMessage}\n";
            var bytes = Encoding.UTF8.GetBytes(fullLogLine);

            // If the log file doesn't exist yet, provision a blank file template placeholder size
            if (!await fileClient.ExistsAsync())
            {
                await fileClient.CreateAsync(maxSize: 1024 * 1024); // Allocate a 1MB file limit footprint
            }

            // Upload and commit the log strings directly into the file stream storage cloud container
            using (var stream = new MemoryStream(bytes))
            {
                await fileClient.UploadAsync(stream);
            }
        }
    }
}
