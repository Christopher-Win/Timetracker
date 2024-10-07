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
        
    }
}
    