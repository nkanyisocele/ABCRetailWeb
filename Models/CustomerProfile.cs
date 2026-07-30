using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABCRetailWeb.Models
{
    public class CustomerProfile : ITableEntity
    {
        // Mandatory Azure Table parameters
        public string PartitionKey { get; set; } = "Customer";
        public string RowKey { get; set; } = string.Empty; // We will map this to the Email address

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Core business data fields
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
    }
}
