using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.Extensions;

namespace TimeTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // Secured endpoint to get the user based on the JWT stored in the cookie
        [HttpGet("me")]
        [Authorize]  // Ensure that the user is authenticated
        public ActionResult<User> GetUserFromJwt()
        {
            // Extract the NetID from the authenticated user's claims
            var netIDClaim = User.GetUserNetId();

            // Get the user by NetID
            var user = _authService.GetByNetId(netIDClaim);

            if (user is not null)
            {
                return Ok(user);
            }

            return NotFound("User not found.");
        }

        // Register a new user
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(user);

            if (result.Success)
            {
                Console.WriteLine($"User created successfully: {user.NetID}");
                return Ok($"User created successfully: {user.NetID}");
            }

            return BadRequest(result.Message);
        }

        // Login a user and set JWT token in cookie
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginRequest request)
        {
            // Ensure NetID and Password are not null or empty
            if (string.IsNullOrEmpty(request.NetID) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "NetID and Password must be provided." });
            }

            var result = await _authService.AuthenticateUser(request.NetID, request.Password); // Result will contain the JWT token in the Message property

            if (result.Success)
            {
                Console.WriteLine($"Login successful for: {request.NetID}");

                // Ensure the result.Message (JWT token) is not null
                if (string.IsNullOrEmpty(result.Message))
                {
                    return StatusCode(500, new { message = "Failed to generate authentication token." });
                }

                // Set the JWT token in the cookies
                Response.Cookies.Append("jwt", result.Message, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // Set to true in production for HTTPS
                    SameSite = SameSiteMode.Lax, // Set to SameSiteMode.Strict in production
                    Expires = DateTime.UtcNow.AddDays(3)
                });
                // Check if the user requires a password change.
                if (result.RequiresPasswordChange)
                {
                    Console.WriteLine($"User {request.NetID} requires password change.");
                    return Ok(new { message = "Login successful", requiresPasswordChange = true }); // Return success message and indicate that the user requires a password change
                }
                return Ok(new { message = "Login successful", requiresPasswordChange = false }); // Return success message and indicate that the user does not require a password change
            }

            // If authentication fails, return Unauthorized
            return Unauthorized(new { message = result.Message }); 
        }
        // PATCH /api/Auth/update-password endpoint to update the user's password
        [HttpPatch("update-password")]
        [Authorize]  // Ensure that the user is authenticated
        public async Task<IActionResult> UpdatePassword([FromForm] UpdatePasswordRequest request)
        {
            // Extract the NetID from the authenticated user's claims
            var netIDClaim = User.GetUserNetId();

            // Get the user by NetID
            var user = _authService.GetByNetId(netIDClaim); 

            if (user is null)
            {
                return NotFound("User not found.");
            }
            //Attempt to update the password
            var result = await _authService.UpdatePasswordAsync(user.NetID, request.Password);
            // Check if the update was successful
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message }); // Return error message
            }
            return Ok(new { message = "Password updated successfully" }); // Return success message
        }
        // Logout the user by removing the JWT cookie
        [HttpPost("logout")]
        [Authorize]  // Ensure that the user is authenticated
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt"); // Remove the JWT cookie from the client's cookies ie., log the user out
            return Ok(new { message = "Logout successful" });
        }

        // Request to reset the user's password
        public class UpdatePasswordRequest
        {
            public string Password { get; set; } = string.Empty;
        }
        public class LoginRequest
        {
            public string NetID { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}