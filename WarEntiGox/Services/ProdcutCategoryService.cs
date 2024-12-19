using MongoDB.Bson;
using MongoDB.Driver;
using WarEntiGox.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WarEntiGox.Services
{
    public class ProductCategoryService
    {
        private readonly IMongoCollection<ProductCategory> _categories;

        public ProductCategoryService(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("WarEntiGox");
            _categories = database.GetCollection<ProductCategory>("ProductCategory");
        }

        // Kategorileri listeleme
        public async Task<List<ProductCategory>> GetAllCategoriesAsync()
        {
            var filter = Builders<ProductCategory>.Filter.Eq(c => c.IsDeleted, false);
            return await _categories.Find(filter).ToListAsync();
        }

        // ID'ye göre kategori getirme
        public async Task<ProductCategory> GetCategoryByIdAsync(ObjectId id)
        {
            return await _categories.Find(category => category.Id == id && !category.IsDeleted).FirstOrDefaultAsync();
        }

        // Kategori oluşturma
        public async Task CreateCategoryAsync(ProductCategory category)
        {
            await _categories.InsertOneAsync(category);
        }

        // Kategori güncelleme
        public async Task UpdateCategoryAsync(ObjectId id, ProductCategory category)
        {
            var update = Builders<ProductCategory>.Update
                .Set(c => c.Name, category.Name)
                .Set(c => c.Description, category.Description)
                .Set(c => c.UpdateDate, category.UpdateDate)
                .Set(c => c.IsDeleted, category.IsDeleted);

            await _categories.UpdateOneAsync(c => c.Id == id && !c.IsDeleted, update);
        }

        // Soft delete işlemi (IsDeleted alanını true yapmak)
        public async Task SoftDeleteCategoryAsync(ObjectId id)
        {
            var update = Builders<ProductCategory>.Update.Set(c => c.IsDeleted, true);
            await _categories.UpdateOneAsync(category => category.Id == id, update);
        }

        // Kategorilerin adlarını döndüren metot
        public async Task<List<string>> GetCategoryNamesAsync()
        {
            var categories = await GetAllCategoriesAsync();
            var categoryNames = categories.Select(c => c.Name).ToList();
            return categoryNames;
        }
    }
}
