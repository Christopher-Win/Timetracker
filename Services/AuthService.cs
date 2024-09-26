using System.Threading.Tasks;
using TimeTracker.Models;
using TimeTracker.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TimeTracker.Services
{
    public class AuthService
    {
        private readonly ApplicationDBContext _context;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _expiry;

        // Constructor that injects ApplicationDBContext and IConfiguration for JWT settings
        public AuthService(ApplicationDBContext context, IConfiguration configuration)
        {
            _context = context;
            _secretKey = configuration["JwtSettings:SecretKey"] ?? throw new ArgumentNullException(nameof(_secretKey), "SecretKey cannot be null.");
            _issuer = configuration["JwtSettings:Issuer"] ?? throw new ArgumentNullException(nameof(_issuer), "Issuer cannot be null.");
            _audience = configuration["JwtSettings:Audience"] ?? throw new ArgumentNullException(nameof(_audience), "Audience cannot be null.");
            _expiry = configuration["JwtSettings:ExpiryInDays"] ?? throw new ArgumentNullException(nameof(_expiry), "Expire date cannot be null.");
        }

        // Retrieve user by NetID
        public User? GetByNetId(string id)
        {
            return _context.Users.FirstOrDefault(u => u.NetID == id);
        }

        // Register a new user
        public async Task<Result> RegisterAsync(User newUser)
        {
            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.NetID == newUser.NetID);
            if (existingUser != null)
            {
                return new Result { Success = false, Message = "NetID is already taken." };
            }

            // Hash the password before saving
            var hashedPassword = HashPassword(newUser.Password);

            var user = new User
            {
                NetID = newUser.NetID,
                Password = hashedPassword,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                CreatedAt = DateTime.Now,
                Role = newUser.Role
            };

            // Add the new user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new Result { Success = true }; // Return success result
        }

        // Authenticate a user by NetID and password
        public async Task<Result> AuthenticateUser(string netID, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.NetID == netID);

            if (user == null || user.Password != HashPassword(password))
            {
                return new Result { Success = false, Message = "Invalid NetID or Password." };
            }

            // Generate JWT token upon successful authentication
            var token = GenerateJwtToken(user);

            return new Result { Success = true, Message = token };
        }

        // Generate JWT token
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.NetID),
                new(ClaimTypes.Role, user.Role) // Add the Role claim here
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(Convert.ToInt32(_expiry)), // Token expiration
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Hash password using SHA256
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Result class for returning operation results
        public class Result
        {
            public bool Success { get; set; }
            public string? Message { get; set; } // Mark Message as nullable
        }
    }
}