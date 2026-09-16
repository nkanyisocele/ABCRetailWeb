using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABCRetail.Functions
{
    public class QueueStorageFunction
    {
        [Function("ProcessQueueMessage")]
        public void Run(
            [QueueTrigger("order-queue", Connection = "AzureWebJobsStorage")] string myQueueItem,
            FunctionContext context)
        {
            var logger = context.GetLogger("ProcessQueueMessage");
            logger.LogInformation($"[Queue Trigger] Processing background event message: {myQueueItem}");
        }
    }
}


