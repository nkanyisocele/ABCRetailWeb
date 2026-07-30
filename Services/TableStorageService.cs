using Azure.Data.Tables;
using ABCRetailWeb.Models;

namespace ABCRetailWeb.Services
{
    public class TableStorageService
    {
        private readonly TableClient _customerTableClient;
        private readonly TableClient _productTableClient;

       
        public TableStorageService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");

            // Connects to and auto-creates tables if they don't already exist in your Azure Portal
            _customerTableClient = new TableClient(connectionString, "CustomerProfiles");
            _customerTableClient.CreateIfNotExists();

            _productTableClient = new TableClient(connectionString, "ProductInformation");
            _productTableClient.CreateIfNotExists();
        }

        // --- CUSTOMER METHODS ---
        public async Task<List<CustomerProfile>> GetAllCustomersAsync()
        {
            var customers = new List<CustomerProfile>();
            var queryResults = _customerTableClient.QueryAsync<CustomerProfile>(filter: "");
            await foreach (var entity in queryResults)
            {
                customers.Add(entity);
            }
            return customers;
        }

        public async Task AddCustomerAsync(CustomerProfile customer)
        {
            // Automatically uses the email address as the unique identifier RowKey
            customer.RowKey = customer.Email;
            await _customerTableClient.AddEntityAsync(customer);
        }

        // --- PRODUCT METHODS ---
        public async Task<List<ProductInfo>> GetAllProductsAsync()
        {
            var products = new List<ProductInfo>();
            var queryResults = _productTableClient.QueryAsync<ProductInfo>(filter: "");
            await foreach (var entity in queryResults)
            {
                products.Add(entity);
            }
            return products;
        }

        public async Task AddProductAsync(ProductInfo product)
        {
            await _productTableClient.AddEntityAsync(product);
        }
    }
}

