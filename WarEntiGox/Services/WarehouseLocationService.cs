using MongoDB.Bson;
using MongoDB.Driver;
using WarEntiGox.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace WarEntiGox.Services
{
    public class WarehouseLocationService
    {
        private readonly IMongoCollection<WarehouseLocation> _warehouseLocations;
        private readonly IMongoCollection<BsonDocument> _counterCollection;

        public WarehouseLocationService(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("WarEntiGox");
            _warehouseLocations = database.GetCollection<WarehouseLocation>("WarehouseLocation");
            _counterCollection = database.GetCollection<BsonDocument>("Counters");
        }

        // Retrieves all warehouse locations for a specific warehouse and company
        public async Task<List<WarehouseLocation>> GetAllWarehouseLocationsAsync(int warehouseId, int companyId)
        {
            var filter = Builders<WarehouseLocation>.Filter.And(
                Builders<WarehouseLocation>.Filter.Eq(wl => wl.IsDeleted, false),
                Builders<WarehouseLocation>.Filter.Eq(wl => wl.WarehouseId, warehouseId),
                Builders<WarehouseLocation>.Filter.Eq(wl => wl.CompanyId, companyId)
            );
            return await _warehouseLocations.Find(filter).ToListAsync();
        }

        // Retrieves a single warehouse location by its ID and company ID
        public async Task<WarehouseLocation> GetWarehouseLocationByIdAsync(ObjectId id, int companyId)
        {
            var filter = Builders<WarehouseLocation>.Filter.And(
                Builders<WarehouseLocation>.Filter.Eq(wl => wl.Id, id),
                Builders<WarehouseLocation>.Filter.Eq(wl => wl.CompanyId, companyId),
                Builders<WarehouseLocation>.Filter.Eq(wl => wl.IsDeleted, false)
            );
            return await _warehouseLocations.Find(filter).FirstOrDefaultAsync();
        }

        // Creates a new warehouse location for a specific warehouse and company
        public async Task CreateWarehouseLocationAsync(WarehouseLocation warehouseLocation)
        {
            // Ensures the location name is unique
            var existingLocation = await _warehouseLocations.Find(wl => wl.LocationName == warehouseLocation.LocationName && wl.WarehouseId == warehouseLocation.WarehouseId && wl.CompanyId == warehouseLocation.CompanyId).FirstOrDefaultAsync();
            if (existingLocation != null)
            {
                throw new InvalidOperationException("A location with the same name already exists for this warehouse.");
            }

            // Get the next WarehouseLocationId from the Counter collection
            warehouseLocation.WarehouseLocationId = await GetNextWarehouseLocationIdAsync();

            await _warehouseLocations.InsertOneAsync(warehouseLocation);
        }

        // Updates an existing warehouse location for a specific warehouse and company
        public async Task UpdateWarehouseLocationAsync(ObjectId id, WarehouseLocation warehouseLocation)
        {
            var update = Builders<WarehouseLocation>.Update
                .Set(wl => wl.LocationName, warehouseLocation.LocationName)
                .Set(wl => wl.Description, warehouseLocation.Description)
                .Set(wl => wl.UpdateDate, warehouseLocation.UpdateDate);

            var result = await _warehouseLocations.UpdateOneAsync(wl => wl.Id == id && wl.WarehouseId == warehouseLocation.WarehouseId && wl.CompanyId == warehouseLocation.CompanyId && !wl.IsDeleted, update);
            if (result.ModifiedCount == 0)
            {
                throw new InvalidOperationException("Warehouse location not found or no changes made.");
            }
        }

        // Performs a soft delete on a warehouse location for a specific warehouse and company
        public async Task SoftDeleteWarehouseLocationAsync(ObjectId id, int warehouseId, int companyId)
        {
            var update = Builders<WarehouseLocation>.Update.Set(wl => wl.IsDeleted, true);
            var result = await _warehouseLocations.UpdateOneAsync(location => location.Id == id && location.WarehouseId == warehouseId && location.CompanyId == companyId, update);
            if (result.MatchedCount == 0)
            {
                throw new InvalidOperationException("Warehouse location not found or no changes made.");
            }
        }

        // Get the next WarehouseLocationId from the Counter collection
        public async Task<int> GetNextWarehouseLocationIdAsync()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Name", "WarehouseLocationId");
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
