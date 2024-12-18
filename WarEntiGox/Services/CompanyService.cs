using MongoDB.Driver;
using WarEntiGox.Models;
using MongoDB.Bson;

namespace WarEntiGox.Services
{
    public class CompanyService
    {
        private readonly IMongoCollection<Company> _companyCollection;

        public CompanyService(IMongoClient client)
        {
            var database = client.GetDatabase("WarEntriGox");
            _companyCollection = database.GetCollection<Company>("Companies");
        }

        public async Task<List<Company>> GetCompaniesAsync()
        {
            var filter = Builders<Company>.Filter.Eq(c => c.IsDeleted, false);
            return await _companyCollection.Find(filter).ToListAsync();
        }

        public async Task<Company> GetCompanyByIdAsync(ObjectId id)
        {
            return await _companyCollection.Find<Company>(company => company.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateCompanyAsync(Company company)
        {
            await _companyCollection.InsertOneAsync(company);
        }

        public async Task UpdateCompanyAsync(ObjectId id, Company company)
        {
            await _companyCollection.ReplaceOneAsync(c => c.Id == id, company);
        }

        public async Task SoftDeleteCompanyAsync(ObjectId id)
        {
            var update = Builders<Company>.Update.Set(c => c.IsDeleted, true);
            await _companyCollection.UpdateOneAsync(company => company.Id == id, update);
        }
    }
}