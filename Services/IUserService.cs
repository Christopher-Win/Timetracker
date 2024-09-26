using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TimeTracker.Models;

namespace TimeTracker.Services
{
    public interface IUserService
    {
        Task ImportUsersFromFileAsync(IFormFile file);
        Task<User> GetUserByNetIdAsync(string netId);
        // Other user-related methods...
    }
}