using TimeTracker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using TimeTracker.Services;
using TimeTracker.Data;
using System.Text;

public class UserService : IUserService
{
    private readonly ApplicationDBContext _context;

    public UserService(ApplicationDBContext context)
    {
        _context = context;
    }
    public async Task<User> GetUserByNetIdAsync(string netId)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.NetID == netId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            return user;
        }

    public async Task ImportUsersFromFileAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        var users = new List<User>();
        
        bool isFirstLine = true;

        while (!reader.EndOfStream)
        {
           
            var line = await reader.ReadLineAsync(); // Read a line from the file
            line = line.Trim();     // Remove leading and trailing whitespace including newline characters
            if (isFirstLine)    // skip the header line
            {
                isFirstLine = false;
                continue; 
            }

            var values = line.Split('\t'); // Use Split to get each value in the line
            Console.WriteLine($"LastName: {values[0].Trim()}, FirstName: {values[1].Trim()}, NetID: {values[2].Trim()}, UTDID: {values[3].Trim()}");
            Console.WriteLine($"Length: {values.Length}");
            if (values.Length != 4)
            {
                // Optionally log or skip invalid lines
                continue;
            }
            // Extract values from the line
            var lastName = values[0].Trim();
            var firstName = values[1].Trim();
            var netId = values[2].Trim();     // Username (NetID)
            var utdId = values[3].Trim();     // Student ID (UTDID)

            // Check for existing user
            if (await _context.Users.AnyAsync(u => u.NetID == netId))
            {
                continue;   // Skip duplicates or handle as needed
            }
            // Create a new User object
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                NetID = netId,
                Password = HashPassword(utdId),
                Role = "Student"
            };

            users.Add(user);// Add the new user to the list
        }

        if (users.Any()) // Check if there are valid users to import
        {
            _context.Users.AddRange(users); // Add all users to the database
            await _context.SaveChangesAsync(); // Save changes
        }
        else
        {
            throw new InvalidOperationException("No valid users to import."); // Throw exception if no valid users
        }
    }

    // Other service methods...
     public static string HashPassword(string password) // Hash the password using SHA-256
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

}