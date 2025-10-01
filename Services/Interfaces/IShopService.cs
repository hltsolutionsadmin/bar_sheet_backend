using BarSheetAPI.DTOs;
using BarSheetAPI.Models;

namespace BarSheetAPI.Services.Interfaces
{
    public interface IShopService
    {
        Task<IEnumerable<Shop>> GetShopsAsync();
        Task<Shop> CreateShopAsync(ShopRegisterDto shop);
    }
}
