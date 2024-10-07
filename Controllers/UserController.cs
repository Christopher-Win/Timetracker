using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Models;
using TimeTracker.Services;
using System.Security.Claims;
using TimeTracker.Extensions;
using TimeTracker.Models.Dto;

namespace TimeTracker.Controllers
{
    [Route("api/[controller]")] // This will be the endpoint: https://baseURL/api/user
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        
        [HttpGet("{netId}")]
        public async Task<IActionResult> GetUser(string netId)
        {
            var user = await _userService.GetUserByNetIdAsync(netId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);

        }
        // PATCH /api/user/{id}
        [Authorize(Roles = "Admin")] // Ensure that the user is authenticated and has the Admin role
        [HttpPatch("{netId}")]
        public async Task<IActionResult> UpdateUserGroup(string netId, [FromBody] UpdateUserGroupDto updateUserGroupDto)
        {
            try
            {
                var result = await _userService.UpdateUserGroupAsync(netId, updateUserGroupDto.Group);

                if (result)
                {
                    return Ok("User group updated successfully.");
                }

                return NotFound("User not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetUsersByGroup(int groupId)
        {
            var users = await _userService.GetUsersByGroupAsync(groupId);

            if (users == null || !users.Any())
            {
                return NotFound("No users found in this group.");
            }

            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("group/{netId}")]
        public async Task<IActionResult> RemoveUserFromGroup(string netId)
        {
            var result = await _userService.UpdateUserGroupAsync(netId, 0); // Assuming '0' is a default for 'no group'

            if (result)
            {
                return Ok("User removed from group successfully.");
            }

            return NotFound("User not found.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();

            if (users == null || !users.Any())
            {
                return NotFound("No users found.");
            }

            return Ok(users);
        }
        
    }
}
    