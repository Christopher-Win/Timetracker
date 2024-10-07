using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TimeTracker.Models;

namespace TimeTracker.Services
{
    public interface IUserService
    {
        Task ImportUsersFromFileAsync(IFormFile file);
        Task<User> GetUserByNetIdAsync(string netId);
        Task<bool> UpdateUserGroupAsync(string netId, int group);
        // Other user-related methods...
    }
}