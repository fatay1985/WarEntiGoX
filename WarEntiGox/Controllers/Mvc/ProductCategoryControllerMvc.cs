using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using WarEntiGox.Models;
using WarEntiGox.Services;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace WarEntiGox.Controllers.MVC
{
    [Route("mvc/[controller]")]
    public class ProductCategoryControllerMvc : Controller
    {
        private readonly ProductCategoryService _categoryService;

        public ProductCategoryControllerMvc(ProductCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // Kategorileri Listele (ve arama işlemi)
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm)
        {
            ViewData["CurrentFilter"] = searchTerm;

            var categories = await _categoryService.GetAllCategoriesAsync();

            if (categories == null)
            {
                categories = new List<ProductCategory>();
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                categories = categories.Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return View("~/Views/ProductCategory/Index.cshtml", categories);
        }

        // Yeni Kategori Oluşturma Sayfası
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("~/Views/ProductCategory/Create.cshtml");
        }

        // Kategori Oluşturma İşlemi
        [HttpPost("Create")]
        public async Task<IActionResult> Create(ProductCategory category)
        {
            if (ModelState.IsValid)
            {
                category.CreateDate = DateTime.Now;
                category.UpdateDate = null; // İlk oluşturduğumuzda null
                category.IsDeleted = false;

                await _categoryService.CreateCategoryAsync(category);
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/ProductCategory/Create.cshtml", category);
        }

        // Kategori Düzenleme Sayfası
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return NotFound();

            var category = await _categoryService.GetCategoryByIdAsync(objectId);
            if (category == null)
                return NotFound();

            return View("~/Views/ProductCategory/Edit.cshtml", category);
        }

        // Kategori Düzenleme İşlemi
        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(string id, ProductCategory category)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return NotFound();

            if (ModelState.IsValid)
            {
                category.UpdateDate = DateTime.Now;

                await _categoryService.UpdateCategoryAsync(objectId, category);
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/ProductCategory/Edit.cshtml", category);
        }

        // Kategori Silme Sayfası
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return NotFound();

            var category = await _categoryService.GetCategoryByIdAsync(objectId);
            if (category == null)
                return NotFound();

            return View("~/Views/ProductCategory/Delete.cshtml", category);
        }

        // Kategori Silme İşlemi (Soft Delete)
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return NotFound();

            // Soft delete işlemi (IsDeleted alanını true yapıyoruz)
            await _categoryService.SoftDeleteCategoryAsync(objectId);

            return RedirectToAction(nameof(Index));
        }
    }
}
