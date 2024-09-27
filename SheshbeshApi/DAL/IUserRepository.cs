using SheshbeshApi.Models;

namespace SheshbeshApi.DAL
{
    public interface IUserRepository
    {
        Task<List<ResponseUser>> GetAsync();
        Task<User?> GetAsync(string id);
        Task CreateAsync(User newUser);
        Task UpdateAsync(string id, User updatedUser);
        Task RemoveAsync(string id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
    }
}
