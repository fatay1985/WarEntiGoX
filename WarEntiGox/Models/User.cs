using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace WarEntiGox.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [SwaggerIgnore]
        public ObjectId Id { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; } // Şirket bilgisi
        public string UserName { get; set; } // Kullanıcı adı
        public string PhoneNumber { get; set; }
        public string Email { get; set; } // E-posta
        public string PasswordHash { get; set; } // Şifre hash'i
        public string Role { get; set; } // Rol
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
