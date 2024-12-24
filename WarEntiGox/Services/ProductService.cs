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

        // Retrieves all products
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var filter = Builders<Product>.Filter.Eq(p => p.IsDeleted, false);
            var products = await _products.Find(filter).ToListAsync();

            // Category fetching can be done in the controller to avoid coupling services too tightly
            return products;
        }

        // Retrieves a single product by its ID
        public async Task<Product> GetProductByIdAsync(ObjectId id)
        {
            var product = await _products.Find(p => p.Id == id && !p.IsDeleted).FirstOrDefaultAsync();
            return product;
        }

        // Creates a new product
        public async Task CreateProductAsync(Product product)
        {
            // Ensures the SKU is unique
            var existingProduct = await _products.Find(p => p.SKU == product.SKU).FirstOrDefaultAsync();
            if (existingProduct != null)
            {
                throw new InvalidOperationException("A product with the same SKU already exists.");
            }

            await _products.InsertOneAsync(product);
        }

        // Updates an existing product
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

            var result = await _products.UpdateOneAsync(p => p.Id == id && !p.IsDeleted, update);
            if (result.ModifiedCount == 0)
            {
                throw new InvalidOperationException("Product not found or no changes made.");
            }
        }

        // Performs a soft delete on a product
        public async Task SoftDeleteProductAsync(ObjectId id)
        {
            var update = Builders<Product>.Update.Set(p => p.IsDeleted, true);
            var result = await _products.UpdateOneAsync(product => product.Id == id, update);
            if (result.MatchedCount == 0)
            {
                throw new InvalidOperationException("Product not found.");
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
            return counter != null ? counter["Value"].AsInt32 : 1;  // Eğer counter yoksa 1 döner
        }
    }
}
