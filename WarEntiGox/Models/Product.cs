using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using System;

namespace WarEntiGox.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [SwaggerIgnore]
        public ObjectId Id { get; set; }
        public int ProductId { get; set; }

        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CompanyId { get; set; } = 1;
        public string SKU { get; set; }
        public string Description { get; set; }

        // Kategori ID, veri tabanında referans olarak saklanıyor
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId CategoryId { get; set; }

        [SwaggerIgnore]
        public bool IsPublished { get; set; }

        [SwaggerIgnore]
        public DateTime CreateDate { get; set; }

        [SwaggerIgnore]
        public DateTime? UpdateDate { get; set; }

        [SwaggerIgnore]
        public bool IsDeleted { get; set; }
    }
}