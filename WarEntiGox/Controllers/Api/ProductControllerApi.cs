using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using WarEntiGox.Models;
using WarEntiGox.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace WarEntiGox.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductControllerApi : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductControllerApi(ProductService productService)
        {
            _productService = productService;
        }

        // GET: api/ProductControllerApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        // GET: api/ProductControllerApi/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductById(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid ID format.");

            var product = await _productService.GetProductByIdAsync(objectId);

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // POST: api/ProductControllerApi
        [HttpPost]
        public async Task<ActionResult> CreateProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            product.CreateDate = DateTime.Now;
            product.UpdateDate = DateTime.Now;
            product.IsDeleted = false;
            product.IsPublished = true;

            await _productService.CreateProductAsync(product);

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id.ToString() }, product);
        }

        // PUT: api/ProductControllerApi/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(string id, [FromBody] Product product)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid ID format.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingProduct = await _productService.GetProductByIdAsync(objectId);
            if (existingProduct == null)
                return NotFound();

            product.Id = objectId;
            product.UpdateDate = DateTime.Now;

            await _productService.UpdateProductAsync(objectId, product);

            return NoContent();
        }

        // DELETE: api/ProductControllerApi/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid ID format.");

            var existingProduct = await _productService.GetProductByIdAsync(objectId);
            if (existingProduct == null)
                return NotFound();

            await _productService.SoftDeleteProductAsync(objectId);

            return NoContent();
        }
    }
}
