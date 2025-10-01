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
    }
}
