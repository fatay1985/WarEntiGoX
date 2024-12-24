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

    // Product listing with category names
    [HttpGet]
    public async Task<IActionResult> Index(string searchTerm)
    {
        ViewData["CurrentFilter"] = searchTerm;

        var products = await _productService.GetAllProductsAsync() ?? new List<Product>();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            products = products.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Kategorileri alıyoruz
        var categories = await _categoryService.GetAllCategoriesAsync();
        var categoryDict = categories.ToDictionary(c => c.Id.ToString(), c => c.Name);

        // Kategori ismini alıyoruz, CategoryId'yi kullanarak
        foreach (var product in products)
        {
            if (categoryDict.ContainsKey(product.CategoryId.ToString()))
            {
                product.Description += " (Kategori: " + categoryDict[product.CategoryId.ToString()] + ")";
            }
        }

        // CategoryDict'i ViewData'ya aktarıyoruz
        ViewData["CategoryDict"] = categoryDict;

        return View("~/Views/Product/Index.cshtml", products);
    }

    // Product creation page
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View("~/Views/Product/Create.cshtml");
    }

    // Product creation action
    [HttpPost("Create")]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Please ensure all fields are filled correctly.");
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId.ToString());
            return View("~/Views/Product/Create.cshtml", product);
        }

        try
        {
            product.CreateDate = DateTime.Now;
            product.UpdateDate = DateTime.Now;
            product.IsDeleted = false;
            product.IsPublished = true;

            await _productService.CreateProductAsync(product);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId.ToString());
            return View("~/Views/Product/Create.cshtml", product);
        }
    }

    // Edit product page
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        var productId = new ObjectId(id);
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
        {
            return NotFound();
        }

        var categories = await _categoryService.GetAllCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId.ToString());

        return View("~/Views/Product/Edit.cshtml", product);
    }

    // Edit product action
    [HttpPost("Edit/{id}")]
    public async Task<IActionResult> Edit(string id, Product product)
    {
        var productId = new ObjectId(id);

        if (productId != product.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Please ensure all fields are filled correctly.");
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId.ToString());
            return View("~/Views/Product/Edit.cshtml", product);
        }

        try
        {
            product.UpdateDate = DateTime.Now;
            await _productService.UpdateProductAsync(productId, product);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId.ToString());
            return View("~/Views/Product/Edit.cshtml", product);
        }
    }

    // Delete product page
    [HttpGet("Delete/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var productId = new ObjectId(id);
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
        {
            return NotFound();
        }

        return View("~/Views/Product/Delete.cshtml", product);
    }

    // Confirm delete product action
    [HttpPost("Delete/{id}")]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var productId = new ObjectId(id);
        try
        {
            await _productService.SoftDeleteProductAsync(productId);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return RedirectToAction(nameof(Index));
        }
    }
}
}