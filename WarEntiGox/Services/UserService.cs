using MongoDB.Driver;
using WarEntiGox.Models;
using MongoDB.Bson;
using BCrypt.Net;  // BCrypt.Net kütüphanesini kullanarak şifre doğrulaması yapıyoruz
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WarEntiGox.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _userCollection;

        public UserService(IMongoClient client)
        {
            var database = client.GetDatabase("WarEntiGox");  // MongoDB veritabanı adı
            _userCollection = database.GetCollection<User>("Users");
        }

        public async Task<List<User>> GetUsersAsync()
        {
            var filter = Builders<User>.Filter.Eq(u => u.IsDeleted, false);
            return await _userCollection.Find(filter).ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(ObjectId id)
        {
            return await _userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateUserAsync(User user)
        {
            // Şifreyi hash'leyelim
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            await _userCollection.InsertOneAsync(user);
        }

        public async Task UpdateUserAsync(ObjectId id, User user)
        {
            // Şifreyi hash'leyelim
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            }

            await _userCollection.ReplaceOneAsync(u => u.Id == id, user);
        }

        public async Task SoftDeleteUserAsync(ObjectId id)
        {
            var update = Builders<User>.Update.Set(u => u.IsDeleted, true);
            await _userCollection.UpdateOneAsync(u => u.Id == id, update);
        }

        // Yeni eklenen ValidateUserAsync metodu
        public async Task<User> ValidateUserAsync(string userName, string password)
        {
            var filter = Builders<User>.Filter.Eq(u => u.UserName, userName);
            var user = await _userCollection.Find(filter).FirstOrDefaultAsync();

            // Eğer kullanıcı bulunduysa ve şifre hash'i doğruysa
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user; // Kullanıcı ve şifre doğrulandı
            }

            return null; // Geçersiz kullanıcı adı veya şifre
        }
    }
}
