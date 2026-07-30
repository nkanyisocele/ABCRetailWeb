using System.Collections.Generic;

namespace ABCRetailWeb.Models
{
    public class DashboardViewModel
    {
        public List<CustomerProfile> Customers { get; set; } = new();
        public List<ProductInfo> Products { get; set; } = new();
        public List<string> ImageUrls { get; set; } = new();
    }
}

