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

    public async Task<ProductSize> UpdateProductSizeAsync(UpdateProductSizeDTO productSize)
    {
      var existingProductSize = await _context.ProductSizes
          .FirstOrDefaultAsync(ps => ps.ProductSizeId == productSize.ProductSizeId);

      if (existingProductSize == null)
      {
        throw new KeyNotFoundException($"ProductSize with ID {productSize.ProductSizeId} not found.");
      }

      // Check for unique constraint violation
      var duplicateProductSize = await _context.ProductSizes
          .FirstOrDefaultAsync(ps => ps.ShopId == productSize.ShopId && ps.Name == productSize.Name && ps.ProductSizeId != productSize.ProductSizeId);

      if (duplicateProductSize != null)
      {
        throw new InvalidOperationException($"A product size with name '{productSize.Name}' already exists for Shop ID {productSize.ShopId}.");
      }

      // Update only the Name and UpdatedAt fields
      existingProductSize.Name = productSize.Name;
      existingProductSize.UpdatedAt = DateTime.UtcNow;

      await _context.SaveChangesAsync();
      return existingProductSize;
    }

    public async Task<bool> DeleteProductSizeAsync(int productSizeId)
    {
      var productSize = await _context.ProductSizes
          .FirstOrDefaultAsync(ps => ps.ProductSizeId == productSizeId);

      if (productSize == null)
      {
        throw new KeyNotFoundException($"ProductSize with ID {productSizeId} not found.");
      }

      // TODO: Need Product model to confirm ProductSizeId property for checking associated products
      // Current code assumes Product has ProductSizeId, which caused CS1061 error
      /*
      var hasProducts = await _context.Products
          .AnyAsync(p => p.ProductSizeId == productSizeId);

      if (hasProducts)
      {
          throw new InvalidOperationException("This size contains products.");
      }
      */

      _context.ProductSizes.Remove(productSize);
      await _context.SaveChangesAsync();
      return true;
    }
  }
}
