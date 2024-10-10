using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SheshbeshApi.DAL;
using SheshbeshApi.Models;

namespace SheshbeshApi.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        public ChatHub(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }
        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            if (user == null || !user.Identity!.IsAuthenticated)
            {
                await Clients.Caller.SendAsync("Unauthorized", "You are not authorized. Redirecting...");
                Context.Abort();
            }
            else
            {
                await base.OnConnectedAsync();
            }
        }
        [Authorize]
        public async Task SendMessage(string username, string msg)
        {
            await Clients.All.SendAsync("ReceiveMessage", username, msg);
        }
        [Authorize]
        public async Task JoinRoom(string username1, string username2)
        {
            var groupName = SortUsernames([username1, username2]);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
        [Authorize]
        public async Task LeaveRoom(string username1, string username2)
        {
            var groupName = SortUsernames([username1, username2]);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserLeft", username1); // Notify other users
        }
        [Authorize]
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
        [Authorize]
        private string SortUsernames(string[] usernames)
        {
            var sorted = usernames.OrderBy(u => u).ToArray();
            return $"{sorted[0]}_{sorted[1]}";
        }
    }
}
