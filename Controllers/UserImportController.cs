// Written by: Chris N.
using Microsoft.AspNetCore.Authorization; // Provides attributes for role-based access control
using Microsoft.AspNetCore.Mvc; // Enables building RESTful APIs
using TimeTracker.Services; // References the service layer for user-related operations

namespace TimeTracker.Controllers
{
    [Authorize(Roles = "Admin")] // Ensures only authenticated users with the Admin role can access this controller
    [ApiController] // Marks this class as a REST API controller, enabling features like automatic model validation
    [Route("api/[controller]")] // Specifies the base URL for all endpoints in this controller (e.g., api/UserImport)
    public class UserImportController : ControllerBase
    {
        private readonly IUserService _userService; // Service to handle operations related to user management

        // Constructor for injecting dependencies
        public UserImportController(IUserService userService)
        {
            _userService = userService; // Assigns the injected IUserService instance to a private field
        }

        // POST: api/UserImport/upload
        // Endpoint to upload a file containing user accounts for bulk import
        [HttpPost("upload")]
        public async Task<IActionResult> UploadUserFile([FromForm] IFormFile file) 
        {
            // Validates that a file has been uploaded and is not empty
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded."); // Returns a 400 status code if no file is provided

            try
            {
                // Calls the service layer to process the uploaded file and import user accounts
                await _userService.ImportUsersFromFileAsync(file); 
                
                // Returns a success message if the import is successful
                return Ok(new { message = "Users imported successfully." });
            }
            catch (InvalidOperationException ex)
            {
                // Handles specific validation or format errors in the uploaded file
                return BadRequest(new { message = ex.Message }); // Returns a 400 status code with the exception message
            }
            catch (Exception)
            {
                // Handles unexpected errors (e.g., file processing or server issues)
                // Logging of the error should ideally happen here (not shown in this code)
                return StatusCode(500, new { message = "An unexpected error occurred." }); // Returns a 500 status code
            }
        }
    }
}