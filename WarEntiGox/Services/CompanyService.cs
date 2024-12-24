using MongoDB.Bson;
using MongoDB.Driver;
using WarEntiGox.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace WarEntiGox.Services
{
    public class CompanyService
    {
        private readonly IMongoCollection<Company> _companyCollection;
        private readonly IMongoCollection<BsonDocument> _counterCollection;

        public CompanyService(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("WarEntiGox");
            _companyCollection = database.GetCollection<Company>("Companies");
            _counterCollection = database.GetCollection<BsonDocument>("Counters");
        }

        // Retrieves all companies
        public async Task<List<Company>> GetAllCompaniesAsync()
        {
            var filter = Builders<Company>.Filter.Eq(c => c.IsDeleted, false);
            return await _companyCollection.Find(filter).ToListAsync();
        }

        // Retrieves a single company by its ID
        public async Task<Company> GetCompanyByIdAsync(ObjectId id)
        {
            var company = await _companyCollection.Find(c => c.Id == id && !c.IsDeleted).FirstOrDefaultAsync();
            return company;
        }

        // Creates a new company
        public async Task CreateCompanyAsync(Company company)
        {
            // Ensure the CompanyId is unique and incremented
            company.CompanyId = await GetNextCompanyIdAsync();

            await _companyCollection.InsertOneAsync(company);
        }

        // Updates an existing company
        public async Task UpdateCompanyAsync(ObjectId id, Company company)
        {
            var update = Builders<Company>.Update
                .Set(c => c.Name, company.Name)
                .Set(c => c.Description, company.Description)
                .Set(c => c.Address, company.Address)
                .Set(c => c.UpdateDate, company.UpdateDate)
                .Set(c => c.IsPublished, company.IsPublished);

            var result = await _companyCollection.UpdateOneAsync(c => c.Id == id && !c.IsDeleted, update);
            if (result.ModifiedCount == 0)
            {
                throw new InvalidOperationException("Company not found or no changes made.");
            }
        }

        // Performs a soft delete on a company
        public async Task SoftDeleteCompanyAsync(ObjectId id)
        {
            var update = Builders<Company>.Update.Set(c => c.IsDeleted, true);
            var result = await _companyCollection.UpdateOneAsync(c => c.Id == id, update);
            if (result.MatchedCount == 0)
            {
                throw new InvalidOperationException("Company not found.");
            }
        }

        // Get the next CompanyId from the Counter collection
        public async Task<int> GetNextCompanyIdAsync()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Name", "CompanyId");
            var update = Builders<BsonDocument>.Update.Inc("Value", 1);
            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            var counter = await _counterCollection.FindOneAndUpdateAsync(filter, update, options);
            return counter != null ? counter["Value"].AsInt32 : 1;  // If no counter, start from 1
        }

    }
}
