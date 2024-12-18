using MongoDB.Driver;
using WarEntiGox.Models;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace WarEntiGox.Services
{
    public class SentenceService
    {
        private readonly IMongoCollection<Sentence> _sentenceCollection;

        public SentenceService(IMongoClient client)
        {
            var database = client.GetDatabase("WarEntiGoxDb");
            _sentenceCollection = database.GetCollection<Sentence>("Sentences");
        }

        public async Task<Sentence> GetSentenceByIdAsync(ObjectId id)
        {
            return await _sentenceCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateSentenceAsync(ObjectId id, string newContent)
        {
            var update = Builders<Sentence>.Update.Set(s => s.Content, newContent);
            await _sentenceCollection.UpdateOneAsync(s => s.Id == id, update);
        }
    }
}