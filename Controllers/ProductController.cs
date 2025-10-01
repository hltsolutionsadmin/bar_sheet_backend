using BarSheetAPI.DTOs;
using BarSheetAPI.Models;
using BarSheetAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BarSheetAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // Get all products for a specific shop
        [HttpGet("{shopId}")]
        [Authorize(Roles = "Admin")] // Both Admin and Employee can view
        public async Task<IActionResult> GetProducts(int shopId)
        {
            var products = await _productService.GetProductsByShopAsync(shopId);
            return Ok(products);
        }

        // Add a new product
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admin can add
        public async Task<IActionResult> AddProduct([FromBody] AddProductDto product)
        {
            var addedProduct = await _productService.AddProductAsync(product);
            return Ok(addedProduct);
        }
        // Update an existing product
        [HttpPut("{productId}")]
        [Authorize(Roles = "Admin")] // Only Admin can update
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] AddProductDto product)
        {
            var updatedProduct = await _productService.UpdateProductAsync(productId, product);
            if (updatedProduct == null)
            {
                return NotFound();
            }
            return Ok(updatedProduct);
        }
    }
}
