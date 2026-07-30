using Azure.Storage.Queues;

namespace ABCRetailWeb.Services
{
    public class QueueStorageService
    {
        private readonly QueueClient _queueClient;
        private const string QueueName = "order-queue";

        // Constructor sets up the cloud client connection string
        public QueueStorageService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            _queueClient = new QueueClient(connectionString, QueueName);
            _queueClient.CreateIfNotExists();
        }

        // Sends a message payload string straight up to the Azure Queue
        public async Task SendMessageAsync(string message)
        {
            if (await _queueClient.ExistsAsync())
            {
                // Base64 encoding ensures message contents never break on complex characters
                var bytes = System.Text.Encoding.UTF8.GetBytes(message);
                var base64Message = Convert.ToBase64String(bytes);

                await _queueClient.SendMessageAsync(base64Message);
            }
        }
    }
}
