
using Microsoft.AspNetCore.SignalR;
using SheshbeshApi.Hubs;

namespace SheshbeshApi.Services
{
    public class ChatService : IChatService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendDmMessage(string msg, string id)
        {
            throw new NotImplementedException();
        }

        public async Task SendMessage(string msg)
        {
            await _hubContext.Clients.All.SendAsync(msg);
        }
    }
}
