using Microsoft.AspNetCore.SignalR;

namespace SheshbeshApi.Hubs
{
    public class ChatHub:Hub
    {
        public async Task SendMessage(string msg)
        {
            await Clients.All.SendAsync("ReceiveMessage", msg);
        }
    }
}
