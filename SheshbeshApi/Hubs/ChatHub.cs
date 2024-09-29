using Microsoft.AspNetCore.SignalR;
using SheshbeshApi.Models;

namespace SheshbeshApi.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IDictionary<string, UserRoomConnection> _connection;

        public ChatHub(IDictionary<string, UserRoomConnection> connection)
        {
            _connection = connection;
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

        public async Task SendGroupMessage(string username1, string username2, string message)
        {
            var groupName = SortUsernames([username1, username2]);
            await Clients.Group($"{groupName}").SendAsync("ReceiveGroupMessage", username1,message);
        }

        private string SortUsernames(string[] usernames)
        {
            var sorted = usernames.OrderBy(u => u).ToArray();
            return $"{sorted[0]}_{sorted[1]}";
        }
    }
}
