using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SheshbeshApi.Models;

namespace SheshbeshApi.DAL
{
    public class UsersService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UsersService(IOptions<UsersDatabaseSettings> usersDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                usersDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                usersDatabaseSettings.Value.DatabaseName);

            _usersCollection = mongoDatabase.GetCollection<User>(
                usersDatabaseSettings.Value.UsersCollectionName);
        }

        public async Task<List<ResponseUser>> GetAsync()
        {
            var users = await _usersCollection.Find(_ => true).ToListAsync();

            var responseUsers = users.Select(user => new ResponseUser
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email
            }).ToList();
            return responseUsers;
        }

        public async Task<User?> GetAsync(string id) =>
            await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(User newUser) =>
            await _usersCollection.InsertOneAsync(newUser);

        public async Task UpdateAsync(string id, User updatedUser) =>
            await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

        public async Task RemoveAsync(string id) =>
            await _usersCollection.DeleteOneAsync(x => x.Id == id);

        public async Task<User?> GetUserByUsernameAsync(string username) =>
            await _usersCollection.Find(x => x.UserName == username).FirstOrDefaultAsync();

        public async Task<User?> GetUserByEmailAsync(string email) =>
            await _usersCollection.Find(x => x.Email == email).FirstOrDefaultAsync();
    }
}
