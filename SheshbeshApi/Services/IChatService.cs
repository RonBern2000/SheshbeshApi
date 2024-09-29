namespace SheshbeshApi.Services
{
    public interface IChatService
    {
        Task SendMessage(string msg);
        Task SendDmMessage(string msg, string id);
    }
}
