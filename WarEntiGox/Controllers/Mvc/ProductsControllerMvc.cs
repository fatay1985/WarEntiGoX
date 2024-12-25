using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WarEntiGox.Models;
using WarEntiGox.Services;
using MongoDB.Bson;

namespace WarEntiGox.Controllers.MVC
{
    [Route("products")]
    public class ProductsControllerMvc : Controller
    {
        private readonly ProductService _productService;
        private readonly ProductCategoryService _categoryService;

        public ProductsControllerMvc(ProductService productService, ProductCategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        // List products for the specific company
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm)
        {
            ViewData["CurrentFilter"] = searchTerm;

            var companyId = HttpContext.Session.GetInt32("CompanyId");

            if (!companyId.HasValue)
            {
                return RedirectToAction("Index", "Login"); // If CompanyId not found, redirect to login
            }

            var products = await _productService.GetAllProductsAsync(companyId.Value);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return View("~/Views/Product/Index.cshtml", products);
        }

        // View product details for a specific company
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(ObjectId id)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            if (!companyId.HasValue)
            {
                return RedirectToAction("Index", "Login"); // If CompanyId not found, redirect to login
            }

            var product = await _productService.GetProductByIdAsync(id, companyId.Value);
            if (product == null)
            {
                return NotFound(); // If product not found, return 404
            }

            return View("~/Views/Product/Details.cshtml", product);
        }

        // Create new product
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            if (!companyId.HasValue)
            {
                return RedirectToAction("Index", "Login"); // If CompanyId not found, redirect to login
            }

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View("~/Views/Product/Create.cshtml");
        }

        // Create product POST
        [HttpPost("Create")]
        public async Task<IActionResult> Create(Product product)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            if (!companyId.HasValue)
            {
                return RedirectToAction("Index", "Login"); // If CompanyId not found, redirect to login
            }

            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId.ToString());
                return View("~/Views/Product/Create.cshtml", product);
            }

            try
            {
                product.CompanyId = companyId.Value; // Assigning the CompanyId to the product
                await _productService.CreateProductAsync(product);
                return RedirectToAction(nameof(Index)); // Redirect to the product list
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId.ToString());
                return View("~/Views/Product/Create.cshtml", product); // On error, show the form again
            }
        }

        // Edit product page
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(ObjectId id)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            if (!companyId.HasValue)
            {
                return RedirectToAction("Index", "Login"); // If CompanyId not found, redirect to login
            }

            var product = await _productService.GetProductByIdAsync(id, companyId.Value);
            if (product == null)
            {
                return NotFound(); // If product not found, return 404
            }

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId.ToString());

            return View("~/Views/Product/Edit.cshtml", product);
        }

        // Edit product POST
        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(ObjectId id, Product product)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            if (!companyId.HasValue)
            {
                return RedirectToAction("Index", "Login"); // If CompanyId not found, redirect to login
            }

            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId.ToString());
                return View("~/Views/Product/Edit.cshtml", product);
            }

            try
            {
                product.CompanyId = companyId.Value; // Assigning the CompanyId to the product
                product.UpdateDate = DateTime.Now;
                await _productService.UpdateProductAsync(id, product);
                return RedirectToAction(nameof(Index)); // Redirect to the product list
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId.ToString());
                return View("~/Views/Product/Edit.cshtml", product); // On error, show the form again
            }
        }

        // Delete product
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(ObjectId id)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            if (!companyId.HasValue)
            {
                return RedirectToAction("Index", "Login"); // If CompanyId not found, redirect to login
            }

            try
            {
                await _productService.SoftDeleteProductAsync(id, companyId.Value);
                return RedirectToAction(nameof(Index)); // Redirect to the product list
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return RedirectToAction(nameof(Index)); // Redirect to the product list on error
            }
        }
    }
}
