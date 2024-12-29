using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;

namespace WarEntiGox.Models
{
    public class Warehouse
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [SwaggerIgnore]
        public ObjectId Id { get; set; }
        public int WarehouseId { get; set; } // Depo ID
        public int CompanyId { get; set; } // Şirket ID (referans)
        public string WarehouseName { get; set; } // Depo Adı
        public string Address { get; set; } // Depo Konumu
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsDeleted { get; set; }

    }
}
