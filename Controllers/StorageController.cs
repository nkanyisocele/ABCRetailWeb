using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ABCRetailWeb.Models; // Maps directly to your models folder structure

namespace ABCRetailWeb.Controllers
{
    public class StorageController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _functionBaseUrl;

        // Visual Studio injects the standard IHttpClientFactory matching modular guidelines
        public StorageController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();

            // Extracts your local URL (e.g. https://localhost:7071/api/) dynamically
            _functionBaseUrl = configuration.GetValue<string>("FunctionAppSettings:BaseUrl")
                               ?? "https://localhost:7071/api/";
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // 1. ASYNCHRONOUS CUSTOMER PROFILE TRANSACTION (TABLE STORAGE TRIGGER)
        [HttpPost]
        public async Task<IActionResult> AddCustomer(CustomerProfile model)
        {
            if (!ModelState.IsValid) return View("Index", model);

            string requestUri = $"{_functionBaseUrl}StoreTableData";
            var jsonPayload = JsonConvert.SerializeObject(model);
            var contentString = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(requestUri, contentString);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Customer saved successfully via Azure Serverless Functions.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to store customer profile using serverless engine.";
            }

            return RedirectToAction("Index");
        }

        // 2. ASYNCHRONOUS PRODUCT INVENTORY TRANSACTION (TABLE STORAGE TRIGGER)
        [HttpPost]
        public async Task<IActionResult> AddProduct(ProductInfo model)
        {
            if (!ModelState.IsValid) return View("Index", model);

            string requestUri = $"{_functionBaseUrl}StoreTableData"; // Shares standard table infrastructure route
            var jsonPayload = JsonConvert.SerializeObject(model);
            var contentString = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(requestUri, contentString);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Product details recorded successfully via Serverless function.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to record product metadata.";
            }

            return RedirectToAction("Index");
        }

        // 3. BINARY FILE STREAM TRANSACTION ENGINE (BLOB STORAGE TRIGGER)
        [HttpPost]
        public async Task<IActionResult> UploadMedia(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a valid file to upload.";
                return RedirectToAction("Index");
            }

            string requestUri = $"{_functionBaseUrl}UploadBlobMedia";

            using var multipartContent = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);

            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            multipartContent.Add(streamContent, "file", file.FileName);

            HttpResponseMessage response = await _httpClient.PostAsync(requestUri, multipartContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Product image binary blob safely written via serverless backend.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to process binary image stream upload.";
            }

            return RedirectToAction("Index");
        }

        // 4. DECOUPLED TRANSACTION MESSAGE INGESTION (QUEUE STORAGE TRIGGER)
        [HttpPost]
        public async Task<IActionResult> ProcessOrder(string orderId, string productCode)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                TempData["ErrorMessage"] = "Order validation data cannot be empty.";
                return RedirectToAction("Index");
            }

            string requestUri = $"{_functionBaseUrl}ProcessQueueMessage";
            var orderData = new { OrderId = orderId, ProductCode = productCode, Timestamp = DateTime.UtcNow };

            var jsonPayload = JsonConvert.SerializeObject(orderData);
            var contentString = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(requestUri, contentString);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Transaction event dispatched to Azure Queue successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to append transactional event message.";
            }

            return RedirectToAction("Index");
        }
    }
}



