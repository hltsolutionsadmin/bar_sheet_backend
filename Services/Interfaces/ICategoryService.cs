
using BarSheetAPI.DTOs;
using BarSheetAPI.Models;

namespace BarSheetAPI.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<Category> AddCategoryAsync(AddCategoryDTO Category);
        Task<IEnumerable<Category>> GetCategorys();
        Task<IEnumerable<Category>> GetProductsByShopAsync(int shopId);
    }
}
