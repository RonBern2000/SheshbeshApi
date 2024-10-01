using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SheshbeshApi.Models;

namespace SheshbeshApi.DAL
{
    public class MessageRepository:IMessageRepository
    {
        private readonly IMongoCollection<Message> _messagesCollection;

        public MessageRepository(IOptions<MessagesDatabaseSettings> messagesDatabaseSettings)
        {
            var mongoClient = new MongoClient(messagesDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(messagesDatabaseSettings.Value.DatabaseName);
            _messagesCollection = mongoDatabase.GetCollection<Message>(messagesDatabaseSettings.Value.MessagesCollectionName);
        }

        public async Task<List<Message>> GetMessagesAsync(string senderUsername, string recipientUsername)
        {
            var filter = Builders<Message>.Filter.Or(
                Builders<Message>.Filter.And(
                    Builders<Message>.Filter.Eq(m => m.SenderUsername, senderUsername),
                    Builders<Message>.Filter.Eq(m => m.RecipientUsername, recipientUsername)
                ),
                Builders<Message>.Filter.And(
                    Builders<Message>.Filter.Eq(m => m.SenderUsername, recipientUsername),
                    Builders<Message>.Filter.Eq(m => m.RecipientUsername, senderUsername)
                )
            );

            return await _messagesCollection.Find(filter)
                .Sort(Builders<Message>.Sort.Ascending(m => m.Timestamp))
                .ToListAsync();
        }

        public async Task CreateAsync(Message newMessage) =>
            await _messagesCollection.InsertOneAsync(newMessage);

        public async Task<Message?> GetAsync(string id) =>
            await _messagesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task RemoveAsync(string id) =>
            await _messagesCollection.DeleteOneAsync(x => x.Id == id);
    }
}
