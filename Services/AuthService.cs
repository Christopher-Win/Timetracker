// *************************************************
// **************** Written by: ********************
// ************** Aayush P. and Chris N. ***********
// *************************************************

using TimeTracker.Models; // References application models like User
using TimeTracker.Data; // Provides access to the database context
using System.Security.Cryptography; // Provides cryptographic functions for hashing passwords
using System.Text; // Enables text encoding for hashing and JWT generation
using Microsoft.EntityFrameworkCore; // Provides ORM features for database access
using Microsoft.IdentityModel.Tokens; // Provides tools for JWT generation and validation
using System.IdentityModel.Tokens.Jwt; // Handles JWT creation and decoding
using System.Security.Claims; // Enables claims-based identity for JWT tokens

namespace TimeTracker.Services
{
    // Service for handling authentication, user registration, and JWT token generation
    public class AuthService
    {
        private readonly ApplicationDBContext _context; // Database context for interacting with user data
        private readonly string _secretKey; // Secret key for JWT signing
        private readonly string _issuer; // JWT issuer
        private readonly string _audience; // JWT audience
        private readonly string _expiry; // Token expiration period in days

        // Constructor that initializes the database context and JWT settings
        public AuthService(ApplicationDBContext context, IConfiguration configuration)
        {
            _context = context; // Injected database context
            _secretKey = configuration["JwtSettings:SecretKey"] ?? throw new ArgumentNullException(nameof(_secretKey), "SecretKey cannot be null.");
            _issuer = configuration["JwtSettings:Issuer"] ?? throw new ArgumentNullException(nameof(_issuer), "Issuer cannot be null.");
            _audience = configuration["JwtSettings:Audience"] ?? throw new ArgumentNullException(nameof(_audience), "Audience cannot be null.");
            _expiry = configuration["JwtSettings:ExpiryInDays"] ?? throw new ArgumentNullException(nameof(_expiry), "Expiry date cannot be null.");
        }

        // Retrieve a user by their NetID
        public User? GetByNetId(string id)
        {
            // Fetches the user from the database where the NetID matches
            return _context.Users.FirstOrDefault(u => u.NetID == id);
        }

        // Register a new user in the system
        public async Task<Result> RegisterAsync(User newUser)
        {
            // Check if the NetID is already taken
            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.NetID == newUser.NetID);
            if (existingUser != null)
            {
                return new Result { Success = false, Message = "NetID is already taken." };
            }

            // Hash the user's password before storing it in the database
            var hashedPassword = HashPassword(newUser.Password);

            // Create a new User object
            var user = new User
            {
                NetID = newUser.NetID,
                Password = hashedPassword, // Store the hashed password
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                CreatedAt = DateTime.Now, // Set the creation timestamp
                Role = newUser.Role ?? "Student", // Default to "Student" if no role is specified
                Group = newUser.Group // Assign the group provided during registration
            };

            // Add the new user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // Persist changes to the database

            return new Result { Success = true }; // Return a successful result
        }

        // Authenticate a user by their NetID and password
        public async Task<Result> AuthenticateUser(string netID, string password)
        {
            // Fetch the user from the database by NetID
            var user = await _context.Users.SingleOrDefaultAsync(u => u.NetID == netID);

            // Verify if the user exists and the password matches
            if (user == null || user.Password != HashPassword(password))
            {
                return new Result { Success = false, Message = "Invalid NetID or Password." };
            }

            // Generate a JWT token upon successful authentication
            var token = GenerateJwtToken(user);

            // Check if the user is still using the default password
            if (user.IsDefaultPassword)
            {
                return new Result
                {
                    Success = true,
                    Message = token, // Return the JWT token
                    RequiresPasswordChange = true // Indicate that the password needs to be updated
                };
            }

            return new Result { Success = true, Message = token, RequiresPasswordChange = false };
        }

        // Update the user's password
        public async Task<Result> UpdatePasswordAsync(string netID, string password)
        {
            // Fetch the user from the database by NetID
            var user = await _context.Users.SingleOrDefaultAsync(u => u.NetID == netID);
            if (user == null) // If the user doesn't exist, return an error
            {
                return new Result { Success = false, Message = "User not found." };
            }

            // Check if the new password matches the current password
            if (user.Password == HashPassword(password))
            {
                return new Result { Success = false, Message = "New password cannot be the same as the old password." };
            }

            // Hash the new password
            var hashedPassword = HashPassword(password);

            // Update the user's password and mark it as no longer default
            user.Password = hashedPassword;
            user.IsDefaultPassword = false;

            await _context.SaveChangesAsync(); // Persist changes to the database

            return new Result { Success = true }; // Return a successful result
        }

        // Generate a JWT token for the authenticated user
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler(); // Utility for creating JWT tokens
            var key = Encoding.UTF8.GetBytes(_secretKey); // Convert the secret key to bytes

            // Define the claims for the token
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.NetID), // Add the user's NetID as a claim
                new(ClaimTypes.Role, user.Role) // Add the user's role as a claim
            };

            // Define the token descriptor with its properties
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), // Include the claims
                Expires = DateTime.UtcNow.AddDays(Convert.ToInt32(_expiry)), // Set the expiration period
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), // Use the secret key for signing
                    SecurityAlgorithms.HmacSha256Signature) // Use HMAC SHA-256 for signing
            };

            // Create the token using the descriptor
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token); // Serialize the token into a string
        }

        // Hash a password using SHA256
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create()) // Create a SHA256 hashing algorithm instance
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password)); // Hash the password
                var builder = new StringBuilder(); // Convert the hash bytes into a hex string
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2")); // Format each byte as a two-digit hex value
                }
                return builder.ToString(); // Return the hashed password as a string
            }
        }

        // Class to represent the result of an operation
        public class Result
        {
            public bool Success { get; set; } // Indicates if the operation succeeded
            public string? Message { get; set; } // Optional message for the result
            public bool RequiresPasswordChange { get; set; } // Indicates if the password needs to be updated
        }
    }
}