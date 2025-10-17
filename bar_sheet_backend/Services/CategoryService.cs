using BarSheetAPI.DTOs;
using BarSheetAPI.Models;
using BarSheetAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BarSheetAPI.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly InventoryDbContext _context;

        public CategoryService(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetCategorys()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetProductsByShopAsync(int shopId)
        {
            return await _context.Categories.Where(p => p.ShopId == shopId).Include(x=>x.Products).ToListAsync();
        }

        public async Task<Category> AddCategoryAsync(AddCategoryDTO Category)
        {
            var newCategory = new Category
            {
                ShopId = Category.ShopId,
                Name = Category.Name,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Category.CreatedBy,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();
            return newCategory;
        }

    public async Task<Category> UpdateCategoryAsync(UpdateCategoryDTO category)
    {
      var existingCategory = await _context.Categories
          .FirstOrDefaultAsync(c => c.Id == category.Id);

      if (existingCategory == null)
      {
        throw new KeyNotFoundException($"Category with ID {category.Id} not found.");
      }

      // Check for unique constraint violation
      var duplicateCategory = await _context.Categories
          .FirstOrDefaultAsync(c => c.ShopId == category.ShopId && c.Name == category.Name && c.Id != category.Id);

      if (duplicateCategory != null)
      {
        throw new InvalidOperationException($"A category with name '{category.Name}' already exists for Shop ID {category.ShopId}.");
      }

      // Update only the Name and UpdatedAt fields
      existingCategory.Name = category.Name;
      existingCategory.UpdatedAt = DateTime.UtcNow;

      await _context.SaveChangesAsync();
      return existingCategory;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
      var category = await _context.Categories
          .Include(c => c.Products)
          .FirstOrDefaultAsync(c => c.Id == id);

      if (category == null)
      {
        throw new KeyNotFoundException($"Category with ID {id} not found.");
      }

      if (category.Products.Any())
      {
        throw new InvalidOperationException("Products available for this category.");
      }

      _context.Categories.Remove(category);
      await _context.SaveChangesAsync();
      return true;
    }
  }
}
