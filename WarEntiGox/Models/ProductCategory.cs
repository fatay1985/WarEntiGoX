using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using System;

namespace WarEntiGox.Models
{
    public class ProductCategory
    {
        [BsonId]  // MongoDB'de otomatik olarak benzersiz Id oluşturulacak
        [BsonRepresentation(BsonType.ObjectId)]  // ObjectId serileştirmesi
        //[SwaggerIgnore]  // Swagger dokümantasyonunda gizle
        public ObjectId Id { get; set; }

        public int CompanyId { get; set; }  // Şirket bilgisi

        public string Name { get; set; }  // Kategori adı

        public string Description { get; set; }  // Kategori açıklaması

        public DateTime CreateDate { get; set; }  // Oluşturulma tarihi

        public DateTime? UpdateDate { get; set; }  // Güncellenme tarihi (null olabilir)

        public bool IsDeleted { get; set; }  // Kategori silindi mi?

        // İlişki: Ürünler
        // Eğer ilişkili ürünler (Product) bilgilerini de bu kategoriye eklemek isterseniz
        // aşağıdaki gibi bir koleksiyon ekleyebilirsiniz.
        // public ICollection<Product> Products { get; set; } 
    }
}