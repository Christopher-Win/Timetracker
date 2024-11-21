// Written by: Chris N.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Services;

namespace TimeTracker.Controllers
{
    [Authorize(Roles = "Admin")] // Ensure that the user is authenticated and has the Admin role
    [ApiController]
    [Route("api/[controller]")] // Base route-> api/UserImport
    public class UserImportController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserImportController(IUserService userService)
        {
            _userService = userService;
        }

        // Endpoint to upload the file containing user accounts
        [HttpPost("upload")]
        public async Task<IActionResult> UploadUserFile([FromForm] IFormFile file) // Must name the parameter 'file' to match the form field name
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                await _userService.ImportUsersFromFileAsync(file); // Call the service method to import users
                return Ok(new { message = "Users imported successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                // Log exception (not shown here)
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}