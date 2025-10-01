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

        // Get all Categorys for a specific shop
        [HttpGet]
        [Authorize(Roles = "Admin")] // Both Admin and Employee can view
        public async Task<IActionResult> GetCategorys()
        {
            var Categorys = await _CategoryService.GetCategorys();
            return Ok(Categorys);
        }

        // Get all products for a specific shop
        [HttpGet("{shopId}")]
        [Authorize(Roles = "Admin")] // Both Admin and Employee can view
        public async Task<IActionResult> GetCateforiesWithProducts(int shopId)
        {
            var products = await _CategoryService.GetProductsByShopAsync(shopId);
            return Ok(products);
        }

        // Add a new Category
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admin can add
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryDTO Category)
        {
            var addedCategory = await _CategoryService.AddCategoryAsync(Category);
            return Ok(addedCategory);
        }
    }
}
