using BarSheetAPI.DTOs;
using BarSheetAPI.Models;
using BarSheetAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BarSheetAPI.Services
{
    public class ProductSizeService :IProductSizeService
    {
        private readonly InventoryDbContext _context;

        public ProductSizeService(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductSize>> GetProductSizes(int shopId)
        {
            return await _context.ProductSizes.Where(x => x.ShopId == shopId).ToListAsync();
        }

        public async Task<ProductSize> AddProductSizeAsync(AddProductSizeDTO productSize)
        {
            var newProductSize = new ProductSize
            {
                Name = productSize.Name,
                IsActive = productSize.IsActive,
                ShopId = productSize.ShopId,
                CreatedBy = productSize.CreatedBy,
                CreatedAt= DateTime.Now,
                UpdatedAt=DateTime.Now
            };
            _context.ProductSizes.Add(newProductSize);
            await _context.SaveChangesAsync();
            return newProductSize;
        }
    }
}
