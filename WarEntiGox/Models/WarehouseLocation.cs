using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace WarEntiGox.Models
{
    public class WarehouseLocation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [SwaggerIgnore]
        public ObjectId Id { get; set; }
        public int WarehouseLocationId { get; set; } // Depo Konumu ID
        public int WarehouseId { get; set; } // Depo ID (referans)
        public int CompanyId { get; set; } // Şirket ID (referans)
        public string LocationName { get; set; } // Konum Adı (Örn: Kat 1, Bölüm A)
        public string Description { get; set; } // Konum Açıklaması
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}