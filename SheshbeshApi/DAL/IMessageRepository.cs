using SheshbeshApi.Models;

namespace SheshbeshApi.DAL
{
    public interface IMessageRepository
    {
        Task<List<Message>> GetMessagesAsync(string senderUsername, string recipientUsername);
        Task CreateAsync(Message newMessage);
        Task<Message?> GetAsync(string id);
        Task RemoveAsync(string id);
    }
}
