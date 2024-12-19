using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;


namespace WarEntiGox.Models
{
    public class Company
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [SwaggerIgnore]
        public ObjectId Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; } // Şirket adı
        public string Address { get; set; } // Şirket adresi
        public string Phone { get; set; } // Telefon
        public string Email { get; set; } // E-posta
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsDeleted { get; set; }

        // İlişkiler
        //public List<Product> Product { get; set; } = new List<Product>();
        //public List<Customer> Customers { get; set; } = new List<Customer>();
        //public List<Sale> Sales { get; set; } = new List<Sale>();
        //public List<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
    }

}
