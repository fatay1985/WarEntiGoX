using MongoDB.Bson;
using MongoDB.Driver;
using WarEntiGox.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace WarEntiGox.Services
{
    public class WarehouseService
    {
        private readonly IMongoCollection<Warehouse> _warehouse;
        private readonly IMongoCollection<BsonDocument> _counterCollection;

        public WarehouseService(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("WarEntiGox");
            _warehouse = database.GetCollection<Warehouse>("Warehouse");
            _counterCollection = database.GetCollection<BsonDocument>("Counters");
        }

        // Retrieves all warehouse for a specific company
        public async Task<List<Warehouse>> GetAllWarehouseAsync(int companyId)
        {
            var filter = Builders<Warehouse>.Filter.And(
                Builders<Warehouse>.Filter.Eq(w => w.IsDeleted, false),
                Builders<Warehouse>.Filter.Eq(w => w.CompanyId, companyId)
            );
            return await _warehouse.Find(filter).ToListAsync();
        }

        // Retrieves a single warehouse by its ID and company ID
        public async Task<Warehouse> GetWarehouseByIdAsync(ObjectId id, int companyId)
        {
            var warehouse = await _warehouse.Find(w => w.Id == id && w.CompanyId == companyId && !w.IsDeleted).FirstOrDefaultAsync();
            return warehouse;
        }

        // Creates a new warehouse for a specific company
        public async Task CreateWarehouseAsync(Warehouse warehouse)
        {
            // Ensures the warehouse name is unique
            var existingWarehouse = await _warehouse.Find(w => w.WarehouseName == warehouse.WarehouseName && w.CompanyId == warehouse.CompanyId).FirstOrDefaultAsync();
            if (existingWarehouse != null)
            {
                throw new InvalidOperationException("A warehouse with the same name already exists for this company.");
            }

            // Get the next WarehouseId from the Counter collection
            warehouse.WarehouseId = await GetNextWarehouseIdAsync();

            // Insert the new warehouse into the database
            await _warehouse.InsertOneAsync(warehouse);
        }

        // Updates an existing warehouse for a specific company
        public async Task UpdateWarehouseAsync(ObjectId id, Warehouse warehouse)
        {
            var update = Builders<Warehouse>.Update
                .Set(w => w.WarehouseName, warehouse.WarehouseName)
                .Set(w => w.Address, warehouse.Address)
                .Set(w => w.UpdateDate, warehouse.UpdateDate);

            var result = await _warehouse.UpdateOneAsync(w => w.Id == id && w.CompanyId == warehouse.CompanyId && !w.IsDeleted, update);
            if (result.ModifiedCount == 0)
            {
                throw new InvalidOperationException("Warehouse not found or no changes made.");
            }
        }

        // Performs a soft delete on a warehouse for a specific company
        public async Task SoftDeleteWarehouseAsync(ObjectId id, int companyId)
        {
            var update = Builders<Warehouse>.Update.Set(w => w.IsDeleted, true);
            var result = await _warehouse.UpdateOneAsync(warehouse => warehouse.Id == id && warehouse.CompanyId == companyId, update);
            if (result.MatchedCount == 0)
            {
                throw new InvalidOperationException("Warehouse not found or no changes made.");
            }
        }

        // Get the next WarehouseId from the Counter collection
        public async Task<int> GetNextWarehouseIdAsync()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Name", "WarehouseId");
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
