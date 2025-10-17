using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace BarSheetAPI.Models
{
    public class Shop
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Identity { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CreatedBy { get; set; }

        // Navigation properties
        public ICollection<User> Users { get; set; }
        public ICollection<Product> Products { get; set; }
        public ICollection<ProductSize> ProductSizes { get; set; }
        public ICollection<Category> Categories { get; set; }
        public ICollection<DailyReport> DailySalesReports { get; set; }
        public ICollection<SaleProduct> SalesProducts { get; set; }
    }
}
