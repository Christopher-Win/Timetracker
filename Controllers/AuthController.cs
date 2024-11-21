// Written by: Aayush P. and Chris N.
using Microsoft.AspNetCore.Authorization; // Provides attributes and features for role-based and policy-based authorization
using Microsoft.AspNetCore.Mvc; // Enables building RESTful APIs
using TimeTracker.Models; // References the application's data models
using TimeTracker.Services; // References application services for authentication logic
using TimeTracker.Extensions; // Contains custom extension methods for user-related operations

namespace TimeTracker.Controllers
{
    [Route("api/[controller]")] // Defines the base route for all endpoints in this controller
    [ApiController] // Indicates that this controller responds to HTTP API requests
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService; // Service for handling authentication logic

        // Constructor for initializing the AuthService dependency
        // Dependency injection ensures that a single instance of AuthService is used across the application
        public AuthController(AuthService authService) // Written by: Chris N.
        {
            _authService = authService;
        }

        // GET: api/Auth/me
        // Retrieves the details of the currently authenticated user using JWT token from cookies
        [HttpGet("me")]
        [Authorize]  // Requires the user to be authenticated via JWT
        public ActionResult<User> GetUserFromJwt() // Written by: Chris N.
        {
            // Extracts the NetID claim from the authenticated user's JWT
            var netIDClaim = User.GetUserNetId(); // Custom extension method for extracting claims

            // Fetches user details from the service using the NetID
            var user = _authService.GetByNetId(netIDClaim);

            if (user is not null) // If the user is found, return their details
            {
                return Ok(user); // HTTP 200 OK with user details
            }

            return NotFound("User not found."); // HTTP 404 Not Found if no user matches the NetID
        }

        // POST: api/Auth/register
        // Registers a new user in the system with provided details
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] User user) // Written by: Chris N.
        {
            // Checks whether the provided user data is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // HTTP 400 Bad Request if validation fails
            }

            // Calls the service method to register the user asynchronously
            var result = await _authService.RegisterAsync(user);

            if (result.Success) // Checks if the registration succeeded
            {
                Console.WriteLine($"User created successfully: {user.NetID}"); // Logs success for debugging
                return Ok($"User created successfully: {user.NetID}"); // HTTP 200 OK with a success message
            }

            return BadRequest(result.Message); // HTTP 400 Bad Request if registration failed
        }

        // POST: api/Auth/login
        // Authenticates a user using NetID and password, and sets a JWT token in the cookies
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginRequest request) // Written by: Chris N.
        {
            // Validates that NetID and password fields are provided
            if (string.IsNullOrEmpty(request.NetID) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "NetID and Password must be provided." }); // Return validation error
            }

            // Authenticates the user and generates a JWT token
            var result = await _authService.AuthenticateUser(request.NetID, request.Password);

            if (result.Success) // If authentication is successful
            {
                Console.WriteLine($"Login successful for: {request.NetID}"); // Logs success for debugging

                if (string.IsNullOrEmpty(result.Message)) // Checks if a JWT token was generated
                {
                    return StatusCode(500, new { message = "Failed to generate authentication token." }); // HTTP 500 Internal Server Error
                }

                // Appends the JWT token to the response cookies
                Response.Cookies.Append("jwt", result.Message, new CookieOptions
                {
                    HttpOnly = true, // Ensures the cookie is only accessible via HTTP requests, not JavaScript
                    Secure = false, // Should be set to true in production for HTTPS
                    SameSite = SameSiteMode.Lax, // Restricts cookie sharing across sites; adjust in production
                    Expires = DateTime.UtcNow.AddDays(3) // Sets the expiration of the cookie to 3 days
                });

                // Returns success message and indicates if a password change is required
                return Ok(new { message = "Login successful", requiresPasswordChange = result.RequiresPasswordChange });
            }

            return Unauthorized(new { message = result.Message }); // HTTP 401 Unauthorized if authentication fails
        }

        // PATCH: api/Auth/update-password
        // Allows the authenticated user to update their password
        [HttpPatch("update-password")]
        [Authorize]  // Requires the user to be authenticated via JWT
        public async Task<IActionResult> UpdatePassword([FromForm] UpdatePasswordRequest request) // Written by: Chris N.
        {
            // Extracts the NetID claim from the authenticated user's JWT
            var netIDClaim = User.GetUserNetId();

            // Fetches the user details from the database using their NetID
            var user = _authService.GetByNetId(netIDClaim);

            if (user is null) // If no user matches the NetID, return an error
            {
                return NotFound("User not found."); // HTTP 404 Not Found
            }

            // Calls the service to update the user's password
            var result = await _authService.UpdatePasswordAsync(user.NetID, request.Password);

            if (!result.Success) // If the password update fails
            {
                return BadRequest(new { message = result.Message }); // HTTP 400 Bad Request with error message
            }

            return Ok(new { message = "Password updated successfully" }); // HTTP 200 OK on successful update
        }

        // POST: api/Auth/logout
        // Logs out the user by removing the JWT token stored in cookies
        [HttpPost("logout")]
        [Authorize]  // Requires the user to be authenticated via JWT
        public IActionResult Logout() // Written by: Aayush P.
        {
            Response.Cookies.Delete("jwt"); // Deletes the JWT cookie, effectively logging the user out
            return Ok(new { message = "Logout successful" }); // HTTP 200 OK with logout confirmation
        }

        // Helper classes for request payloads

        // Represents the request payload for updating a user's password
        public class UpdatePasswordRequest // Written by: Chris N.
        {
            public string Password { get; set; } = string.Empty; // Stores the new password
        }

        // Represents the request payload for logging in a user
        public class LoginRequest // Written by: Chris N.
        {
            public string NetID { get; set; } = string.Empty; // Stores the user's NetID
            public string Password { get; set; } = string.Empty; // Stores the user's password
        }
    }
}