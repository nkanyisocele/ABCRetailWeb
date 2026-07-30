using Microsoft.AspNetCore.Mvc;
using ABCRetailWeb.Models;
using ABCRetailWeb.Services;

namespace ABCRetailWeb.Controllers
{
    public class StorageController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly BlobStorageService _blobStorageService;
        private readonly QueueStorageService _queueStorageService;
        private readonly FileShareLoggingService _fileLoggingService;

        public StorageController(
            TableStorageService tableStorageService,
            BlobStorageService blobStorageService,
            QueueStorageService queueStorageService,
            FileShareLoggingService fileLoggingService)
        {
            _tableStorageService = tableStorageService;
            _blobStorageService = blobStorageService;
            _queueStorageService = queueStorageService;
            _fileLoggingService = fileLoggingService;
        }

        // GET: /Storage
        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                Customers = await _tableStorageService.GetAllCustomersAsync(),
                Products = await _tableStorageService.GetAllProductsAsync(),
                ImageUrls = await _blobStorageService.GetAllBlobsAsync()
            };
            return View(model);
        }

        // POST: /Storage/AddCustomer
        [HttpPost]
        public async Task<IActionResult> AddCustomer(CustomerProfile customer)
        {
            if (ModelState.IsValid)
            {
                await _tableStorageService.AddCustomerAsync(customer);

                string msg = $"New Customer Added: {customer.FirstName} {customer.LastName} ({customer.Email})";
                await _queueStorageService.SendMessageAsync(msg);

                // WRITE TO AZURE FILES: Store a distinct text log file record entry
                await _fileLoggingService.WriteLogAsync($"customer-{Guid.NewGuid().ToString().Substring(0, 8)}.txt", msg);
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Storage/AddProduct
        [HttpPost]
        public async Task<IActionResult> ReportAction(ProductInfo product)
        {
            // Fallback route mapping support for action endpoints
            return await AddProduct(product);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(ProductInfo product)
        {
            if (ModelState.IsValid)
            {
                product.RowKey = Guid.NewGuid().ToString();
                await _tableStorageService.AddProductAsync(product);

                string msg = $"New Catalog Product Created: {product.ProductName} - Price: {product.Price:C}";
                await _queueStorageService.SendMessageAsync(msg);

                // WRITE TO AZURE FILES: Store a distinct text log file record entry
                await _fileLoggingService.WriteLogAsync($"product-{Guid.NewGuid().ToString().Substring(0, 8)}.txt", msg);
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Storage/UploadImage
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                using (var stream = file.OpenReadStream())
                {
                    await _blobStorageService.UploadBlobAsync(stream, uniqueFileName);
                }

                string msg = $"Multimedia Upload Completed: Image file '{uniqueFileName}' added to blob cluster storage container.";
                await _queueStorageService.SendMessageAsync(msg);

                // WRITE TO AZURE FILES: Store a distinct text log file record entry
                await _fileLoggingService.WriteLogAsync($"media-{Guid.NewGuid().ToString().Substring(0, 8)}.txt", msg);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}



