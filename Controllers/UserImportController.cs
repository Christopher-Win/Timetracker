using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Services;

namespace TimeTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UserImportController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserImportController(IUserService userService)
        {
            _userService = userService;
        }

        // Endpoint to upload the file containing user accounts
        [HttpPost("upload")]
        public async Task<IActionResult> UploadUserFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                await _userService.ImportUsersFromFileAsync(file);
                return Ok(new { message = "Users imported successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log exception (not shown here)
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}