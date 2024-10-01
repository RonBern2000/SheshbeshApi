using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SheshbeshApi.DAL;
using SheshbeshApi.Models;

namespace SheshbeshApi.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        public ChatHub(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }
        public async Task SendMessage(string username, string msg)
        {
            await Clients.All.SendAsync("ReceiveMessage", username, msg);
        }

        public async Task JoinRoom(string username1, string username2)
        {
            var groupName = SortUsernames([username1, username2]);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveRoom(string username1, string username2)
        {
            var groupName = SortUsernames([username1, username2]);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserLeft", username1); // Notify other users
        }

        public async Task SendGroupMessage(string username1, string username2, string message)
        {
            var groupName = SortUsernames([username1, username2]);
            await Clients.Group($"{groupName}").SendAsync("ReceiveGroupMessage", username1, message);

            var newMessage = new Message
            {
                SenderUsername = username1,
                RecipientUsername = username2,
                MessageContent = message,
                Timestamp = DateTime.UtcNow
            };

            await _messageRepository.CreateAsync(newMessage);
        }

        private string SortUsernames(string[] usernames)
        {
            var sorted = usernames.OrderBy(u => u).ToArray();
            return $"{sorted[0]}_{sorted[1]}";
        }
    }
}
