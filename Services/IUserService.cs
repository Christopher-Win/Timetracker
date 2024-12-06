// Provides asynchronous methods for managing user-related operations
using System.Threading.Tasks; // Enables asynchronous programming
using Microsoft.AspNetCore.Http; // Provides classes for handling HTTP requests, including file uploads
using TimeTracker.Models; // References the User model for database operations

namespace TimeTracker.Services
{
    // Interface defining the contract for user-related operations
    public interface IUserService
    {
        // Imports user data from a file and processes it.
        // Takes an uploaded file (IFormFile) as input and performs the import asynchronously.
        Task ImportUsersFromFileAsync(IFormFile file);

        // Retrieves a User object by their unique NetID.
        // Takes the NetID as input and returns the User object if found, otherwise null.
        Task<User> GetUserByNetIdAsync(string netId);

        // Updates the group assignment for a user identified by their NetID.
        // Takes the NetID and the new group ID as inputs and returns a boolean indicating success or failure.
        Task<bool> UpdateUserGroupAsync(string netId, int group);

        // Retrieves all users belonging to a specific group.
        // Takes the group ID as input and returns a list of User objects in that group.
        Task<List<User>> GetUsersByGroupAsync(int group);

        // Retrieves all users in the system.
        // Returns a list of User objects asynchronously.
        Task<List<User>> GetAllUsersAsync();

        // Retrieves all groups along with the users associated with each group.
        // Returns a dictionary where the key is the group ID and the value is a list of User objects.
        Task<Dictionary<int, List<User>>> GetUsersByGroupAsync();

        // Checks if two users belong to the same group.
        // Takes the NetIDs of the reviewer and reviewee as inputs and returns a boolean indicating the result.
        Task<bool> AreUsersInSameGroupAsync(string reviewerNetId, string revieweeNetId);
    }
}