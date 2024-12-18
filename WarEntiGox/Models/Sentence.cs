using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WarEntiGox.Models
{
    public class Sentence
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Content { get; set; }

        public bool IsDeleted { get; set; } // Silinmiş kullanıcılar için
    }
}