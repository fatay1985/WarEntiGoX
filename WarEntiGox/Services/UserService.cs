using MongoDB.Bson;
using MongoDB.Driver;
using WarEntiGox.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace WarEntiGox.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<BsonDocument> _counterCollection;

        public UserService(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("WarEntiGox");
            _userCollection = database.GetCollection<User>("Users");
            _counterCollection = database.GetCollection<BsonDocument>("Counters");
        }

        // Get all users
        public async Task<List<User>> GetAllUsersAsync()
        {
            var filter = Builders<User>.Filter.Eq(u => u.IsDeleted, false);
            return await _userCollection.Find(filter).ToListAsync();
        }

        // Get a single user by ID
        public async Task<User> GetUserByIdAsync(ObjectId id)
        {
            var user = await _userCollection.Find(u => u.Id == id && !u.IsDeleted).FirstOrDefaultAsync();
            return user;
        }

        // Create a new user with a unique ID
        public async Task CreateUserAsync(User user)
        {
            // Generate a unique UserId using the counter collection
            user.UserId = await GetNextUserIdAsync();
            user.CreateDate = DateTime.Now;
            user.IsDeleted = false;

            await _userCollection.InsertOneAsync(user);
        }

        // Update an existing user
        public async Task UpdateUserAsync(ObjectId id, User user)
        {
            var update = Builders<User>.Update
                .Set(u => u.UserName, user.UserName)
                .Set(u => u.Email, user.Email)
                .Set(u => u.Role, user.Role)
                .Set(u => u.UpdateDate, user.UpdateDate)
                .Set(u => u.IsDeleted, user.IsDeleted);

            var result = await _userCollection.UpdateOneAsync(u => u.Id == id && !u.IsDeleted, update);
            if (result.ModifiedCount == 0)
            {
                throw new InvalidOperationException("User not found or no changes made.");
            }
        }

        // Soft delete a user
        public async Task SoftDeleteUserAsync(ObjectId id)
        {
            var update = Builders<User>.Update.Set(u => u.IsDeleted, true);
            var result = await _userCollection.UpdateOneAsync(u => u.Id == id, update);
            if (result.MatchedCount == 0)
            {
                throw new InvalidOperationException("User not found.");
            }
        }

        // Get the next UserId from the Counter collection
        public async Task<int> GetNextUserIdAsync()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Name", "UserId");
            var update = Builders<BsonDocument>.Update.Inc("Value", 1);
            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            var counter = await _counterCollection.FindOneAndUpdateAsync(filter, update, options);
            return counter != null ? counter["Value"].AsInt32 : 1;  // If no counter, start from 1
        }

        // Validate user credentials
        public async Task<User> ValidateUserAsync(string userName, string password)
        {
            var filter = Builders<User>.Filter.Eq(u => u.UserName, userName);
            var user = await _userCollection.Find(filter).FirstOrDefaultAsync();

            // If the user is found and the password is correct
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user; // Return the user if authentication is successful
            }

            return null; // Return null if the username or password is incorrect
        }
    }
}
