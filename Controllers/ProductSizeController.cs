using BarSheetAPI.DTOs;
using BarSheetAPI.Models;
using BarSheetAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BarSheetAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductSizeController : ControllerBase
    {
        private readonly IProductSizeService _ProductSizeService;

        public ProductSizeController(IProductSizeService ProductSizeService)
        {
            _ProductSizeService = ProductSizeService;
        }

        // Get all ProductSizes for a specific shop
        [HttpGet("{shopId}")]
        [Authorize(Roles = "Admin")] // Both Admin and Employee can view
        public async Task<IActionResult> GetProductSizes(int shopId)
        {
            var ProductSizes = await _ProductSizeService.GetProductSizes(shopId);
            return Ok(ProductSizes);
        }

        // Add a new ProductSize
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admin can add
        public async Task<IActionResult> AddProductSize([FromBody] AddProductSizeDTO ProductSize)
        {
            var addedProductSize = await _ProductSizeService.AddProductSizeAsync(ProductSize);
            return Ok(addedProductSize);
        }
    }
}
