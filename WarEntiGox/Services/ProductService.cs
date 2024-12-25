using MongoDB.Bson;
using MongoDB.Driver;
using WarEntiGox.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace WarEntiGox.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _products;
        private readonly IMongoCollection<BsonDocument> _counterCollection;
        private readonly ProductCategoryService _categoryService;

        public ProductService(IMongoClient mongoClient, ProductCategoryService categoryService)
        {
            var database = mongoClient.GetDatabase("WarEntiGox");
            _products = database.GetCollection<Product>("Product");
            _categoryService = categoryService;
            _counterCollection = database.GetCollection<BsonDocument>("Counters");
        }

        // Retrieves all products for a specific company
        public async Task<List<Product>> GetAllProductsAsync(int companyId)
        {
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Eq(p => p.IsDeleted, false),
                Builders<Product>.Filter.Eq(p => p.CompanyId, companyId)
            );
            return await _products.Find(filter).ToListAsync();
        }

        // Retrieves a single product by its ID and company ID
        public async Task<Product> GetProductByIdAsync(ObjectId id, int companyId)
        {
            var product = await _products.Find(p => p.Id == id && p.CompanyId == companyId && !p.IsDeleted).FirstOrDefaultAsync();
            return product;
        }

        // Creates a new product for a specific company
        public async Task CreateProductAsync(Product product)
        {
            // Ensures the SKU is unique
            var existingProduct = await _products.Find(p => p.SKU == product.SKU && p.CompanyId == product.CompanyId).FirstOrDefaultAsync();
            if (existingProduct != null)
            {
                throw new InvalidOperationException("A product with the same SKU already exists for this company.");
            }

            await _products.InsertOneAsync(product);
        }

        // Updates an existing product for a specific company
        public async Task UpdateProductAsync(ObjectId id, Product product)
        {
            var update = Builders<Product>.Update
                .Set(p => p.Name, product.Name)
                .Set(p => p.Price, product.Price)
                .Set(p => p.StockQuantity, product.StockQuantity)
                .Set(p => p.Description, product.Description)
                .Set(p => p.CategoryId, product.CategoryId)
                .Set(p => p.SKU, product.SKU)
                .Set(p => p.UpdateDate, product.UpdateDate)
                .Set(p => p.IsPublished, product.IsPublished);

            var result = await _products.UpdateOneAsync(p => p.Id == id && p.CompanyId == product.CompanyId && !p.IsDeleted, update);
            if (result.ModifiedCount == 0)
            {
                throw new InvalidOperationException("Product not found or no changes made.");
            }
        }

        // Performs a soft delete on a product for a specific company
        public async Task SoftDeleteProductAsync(ObjectId id, int companyId)
        {
            var update = Builders<Product>.Update.Set(p => p.IsDeleted, true);
            var result = await _products.UpdateOneAsync(product => product.Id == id && product.CompanyId == companyId, update);
            if (result.MatchedCount == 0)
            {
                throw new InvalidOperationException("Product not found or no changes made.");
            }
        }

        // Get the next ProductId from the Counter collection
        public async Task<int> GetNextProductIdAsync()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Name", "ProductId");
            var update = Builders<BsonDocument>.Update.Inc("Value", 1);
            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            var counter = await _counterCollection.FindOneAndUpdateAsync(filter, update, options);
            return counter != null ? counter["Value"].AsInt32 : 1;  // If counter is missing, return 1
        }
    }
}
