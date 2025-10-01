using BarSheetAPI.DTOs;
using BarSheetAPI.Models;
using BarSheetAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BarSheetAPI.Services
{
  public class ProductService : IProductService
  {
    private readonly InventoryDbContext _context;

    public ProductService(InventoryDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<Product>> GetProductsByShopAsync(int shopId)
    {
      return await _context.Products
          .Where(p => p.ShopId == shopId)
          .ToListAsync();
    }

    public async Task<Product> AddProductAsync(AddProductDto product)
    {
      if (product == null) throw new ArgumentNullException(nameof(product));
      if (product.Variants == null || !product.Variants.Any())
        throw new ArgumentException("At least one variant must be provided.");

      var variants = product.Variants.Select(v => new ProductVariant
      {
        SizeId = v.SizeId,
        Price = v.Price,
        Quantity = v.Quantity
      }).ToList();

      var newProduct = new Product
      {
        Name = product.Name,
        CategoryId = product.CategoryId,
        ShopId = product.ShopId,
        CreatedBy = product.CreatedBy,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Variants = variants
      };

      _context.Products.Add(newProduct);
      await _context.SaveChangesAsync();

      return newProduct;
    }

    public async Task<Product> UpdateProductAsync(int productId, AddProductDto product)
    {
      var existingProduct = await _context.Products.FindAsync(productId);
      if (existingProduct == null) return null;

      if (product.Variants == null || !product.Variants.Any())
        throw new ArgumentException("At least one variant must be provided.");

      var variants = product.Variants.Select(v => new ProductVariant
      {
        SizeId = v.SizeId,
        Price = v.Price,
        Quantity = v.Quantity
      }).ToList();

      existingProduct.Name = product.Name;
      existingProduct.CategoryId = product.CategoryId;
      existingProduct.UpdatedAt = DateTime.UtcNow;
      existingProduct.Variants = variants;

      _context.Products.Update(existingProduct);
      await _context.SaveChangesAsync();

      return existingProduct;
    }
  }
}
