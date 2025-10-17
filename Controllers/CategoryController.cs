using BarSheetAPI.DTOs;
using BarSheetAPI.Models;
using BarSheetAPI.Services;
using BarSheetAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BarSheetAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _CategoryService;

        public CategoryController(ICategoryService CategoryService) 
        {
            _CategoryService = CategoryService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCategorys()
        {
            var Categorys = await _CategoryService.GetCategorys();
            return Ok(Categorys);
        }

        [HttpGet("{shopId}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> GetCateforiesWithProducts(int shopId)
        {
            var products = await _CategoryService.GetProductsByShopAsync(shopId);
            return Ok(products);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryDTO Category)
        {
            var addedCategory = await _CategoryService.AddCategoryAsync(Category);
            return Ok(addedCategory);
        }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDTO category)
    {
      if (id != category.Id)
      {
        return BadRequest("Category ID in URL does not match the ID in the request body.");
      }

      try
      {
        var updatedCategory = await _CategoryService.UpdateCategoryAsync(category);
        return Ok(updatedCategory);
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

    // Delete a Category
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
      try
      {
        var deleted = await _CategoryService.DeleteCategoryAsync(id);
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
