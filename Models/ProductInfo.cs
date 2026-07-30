using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABCRetailWeb.Models
{
    public class ProductInfo : ITableEntity
    {
        // Mandatory Azure Table parameters
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = string.Empty; // We will map this to a unique Product SKU

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Core business data fields
        [Required]
        public string ProductName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(0.01, 100000.00)]
        public decimal Price { get; set; }
    }
}
