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

    [HttpPut("{productSizeId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProductSize(int productSizeId, [FromBody] UpdateProductSizeDTO productSize)
    {
      if (productSizeId != productSize.ProductSizeId)
      {
        return BadRequest("ProductSize ID in URL does not match the ID in the request body.");
      }

      try
      {
        var updatedProductSize = await _ProductSizeService.UpdateProductSizeAsync(productSize);
        return Ok(updatedProductSize);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(ex.Message);
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(ex.Message);
      }
    }

    // Delete a ProductSize
    [HttpDelete("{productSizeId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProductSize(int productSizeId)
    {
      try
      {
        var deleted = await _ProductSizeService.DeleteProductSizeAsync(productSizeId);
        return NoContent();
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(ex.Message);
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(ex.Message);
      }
    }
  }
}
