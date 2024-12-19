using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using WarEntiGox.Models;
using WarEntiGox.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WarEntiGox.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductCategorControllerApi : ControllerBase
    {
        private readonly ProductCategoryService _categoryService;

        public ProductCategorControllerApi(ProductCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/ProductCategoryApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductCategory>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        // GET: api/ProductCategoryApi/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductCategory>> GetCategoryById(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid ID format.");

            var category = await _categoryService.GetCategoryByIdAsync(objectId);

            if (category == null)
                return NotFound();

            return Ok(category);
        }

        // POST: api/ProductCategoryApi
        [HttpPost]
        public async Task<ActionResult> CreateCategory([FromBody] ProductCategory category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            category.CreateDate = DateTime.Now;
            category.UpdateDate = DateTime.Now;
            category.IsDeleted = false;

            await _categoryService.CreateCategoryAsync(category);

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id.ToString() }, category);
        }

        // PUT: api/ProductCategoryApi/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCategory(string id, [FromBody] ProductCategory category)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid ID format.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingCategory = await _categoryService.GetCategoryByIdAsync(objectId);
            if (existingCategory == null)
                return NotFound();

            category.Id = objectId;
            category.UpdateDate = DateTime.Now;
            await _categoryService.UpdateCategoryAsync(objectId, category);

            return NoContent(); // 204 No Content döner.
        }

        // DELETE: api/ProductCategoryApi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return NotFound();  // ID formatı geçersizse, 404 döndürür.

            // Veritabanında mevcut kategori var mı kontrol edin
            var existingCategory = await _categoryService.GetCategoryByIdAsync(objectId);
            if (existingCategory == null)
                return NotFound();  // Kategori bulunamadıysa, 404 döndürür.

            // Gerçekten silmek yerine sadece IsDeleted'ı true yapıyoruz
            await _categoryService.SoftDeleteCategoryAsync(objectId);

            // İşlem başarılı olduğunda, NoContent (204) döndürülür.
            return NoContent();  // 204 No Content yanıtı döndürülür.
        }
    }
}
