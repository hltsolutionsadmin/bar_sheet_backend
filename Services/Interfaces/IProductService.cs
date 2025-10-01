using BarSheetAPI.DTOs;
using BarSheetAPI.Models;

namespace BarSheetAPI.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProductsByShopAsync(int shopId);
        Task<Product> AddProductAsync(AddProductDto product);
        Task<Product> UpdateProductAsync(int productId, AddProductDto product);
    }
}
