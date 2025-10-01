using BarSheetAPI.DTOs;
using BarSheetAPI.Models;

namespace BarSheetAPI.Services.Interfaces
{
    public interface IProductSizeService
    {
        Task<ProductSize> AddProductSizeAsync(AddProductSizeDTO ProductSize);
        Task<IEnumerable<ProductSize>> GetProductSizes(int shopId);
    }
}
