using BarSheetAPI.DTOs;
using BarSheetAPI.Models;
using BarSheetAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BarSheetAPI.Services
{
    public class ShopService : IShopService
    {
        private readonly InventoryDbContext _context;

        public ShopService(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Shop>> GetShopsAsync()
        {
            return await _context.Shops.ToListAsync();
        }

        public async Task<Shop> CreateShopAsync(ShopRegisterDto shop)
        {
            var shopDetails = new Shop{ 
                Address = shop.Address, 
                ContactNumber=shop.ContactNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Identity = shop.Identity,
                Name = shop.Name
            };
            _context.Shops.Add(shopDetails);
            await _context.SaveChangesAsync();
            return shopDetails;
        }
    }
}
