// *************************************************
// **************** Written by: ********************
// ************** Aayush P. and Chris N. ***********
// *************************************************

using Microsoft.AspNetCore.Authorization; // Provides role-based and policy-based authorization
using Microsoft.AspNetCore.Mvc; // Enables building RESTful APIs
using TimeTracker.Services; // References service layer for user-related operations
using TimeTracker.Models.Dto; // Contains Data Transfer Objects (DTOs) like UpdateUserGroupDto

namespace TimeTracker.Controllers
{
    // Base URL: api/user
    [Route("api/[controller]")]
    [ApiController] // Identifies this class as a REST API controller, enabling features like automatic model validation
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService; // Handles business logic related to users

        // Constructor for injecting the IUserService dependency
        public UserController(IUserService userService)
        {
            _userService = userService; // Assigns the injected IUserService instance to a private field
        }

        // GET: api/user/{netId}
        // Retrieves details for a specific user by their NetID
        [HttpGet("{netId}")]
        public async Task<IActionResult> GetUser(string netId)
        {
            // Fetches the user details from the service using the provided NetID
            var user = await _userService.GetUserByNetIdAsync(netId);

            if (user == null) // If the user is not found, return HTTP 404 Not Found
            {
                return NotFound("User not found.");
            }

            // If the user exists, return their details with HTTP 200 OK
            return Ok(user);
        }

        // PATCH: api/user/{netId}
        // Updates the group of a specific user (restricted to Admin role)
        [Authorize(Roles = "Admin")] // Ensures only authenticated users with the Admin role can access this endpoint
        [HttpPatch("{netId}")]
        public async Task<IActionResult> UpdateUserGroup(string netId, [FromBody] UpdateUserGroupDto updateUserGroupDto)
        {
            try
            {
                // Attempts to update the user's group via the service layer
                var result = await _userService.UpdateUserGroupAsync(netId, updateUserGroupDto.Group);

                if (result) // If the update is successful, return HTTP 200 OK with a success message
                {
                    return Ok("User group updated successfully.");
                }

                // If the user is not found, return HTTP 404 Not Found
                return NotFound("User not found.");
            }
            catch (Exception ex) // Catch and handle any unexpected errors
            {
                // Return HTTP 500 Internal Server Error with the exception message
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/user/group/{groupId}
        // Retrieves all users belonging to a specific group (restricted to Admin role)
        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetUsersByGroup(int groupId)
        {
            // Fetches the users in the specified group via the service layer
            var users = await _userService.GetUsersByGroupAsync(groupId);

            if (users == null || !users.Any()) // If no users are found, return HTTP 404 Not Found
            {
                return NotFound("No users found in this group.");
            }

            // If users are found, return them with HTTP 200 OK
            return Ok(users);
        }

        // DELETE: api/user/group/{netId}
        // Removes a user from their group (restricted to Admin role)
        [Authorize(Roles = "Admin")] // Ensures only Admin users can access this endpoint
        [HttpDelete("group/{netId}")]
        public async Task<IActionResult> RemoveUserFromGroup(string netId)
        {
            // Attempts to remove the user from their group by setting their group to a default value (e.g., 0)
            var result = await _userService.UpdateUserGroupAsync(netId, 0);

            if (result) // If the removal is successful, return HTTP 200 OK with a success message
            {
                return Ok("User removed from group successfully.");
            }

            // If the user is not found, return HTTP 404 Not Found
            return NotFound("User not found.");
        }

        // GET: api/user/all
        // Retrieves all users in the system (restricted to Admin role)
        [Authorize(Roles = "Admin")] // Ensures only Admin users can access this endpoint
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            // Fetches all users from the service layer
            var users = await _userService.GetAllUsersAsync();

            if (users == null || !users.Any()) // If no users are found, return HTTP 404 Not Found
            {
                return NotFound("No users found.");
            }

            // If users are found, return them with HTTP 200 OK
            return Ok(users);
        }

        // GET: api/user/groups
        // Retrieves all groups and their associated users (no explicit role required)
        [HttpGet("groups")]
        public async Task<IActionResult> GetUsersByGroupAsync()
        {
            // Fetches all groups and their associated users via the service layer
            var groups = await _userService.GetUsersByGroupAsync();

            // Returns the groups with HTTP 200 OK
            return Ok(groups);
        }
    }
}