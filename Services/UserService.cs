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
        _context = context; // Inject the database context
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

    // Upload a file with user data and create user accounts in the database
    public async Task ImportUsersFromFileAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream(); // Open a stream to read the file
        using var reader = new StreamReader(stream); // Create a StreamReader to read the stream
        var users = new List<User>(); // Create a list to store the new users
        
        bool isFirstLine = true;

        while (!reader.EndOfStream) // Loop until the end of the file
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

    public async Task<bool> UpdateUserGroupAsync(string netId, int group)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.NetID == netId); // Find the user by NetID
        if (user == null)
        {
            return false; // User not found
        }
        user.Group = group; // Update the user's group
        await _context.SaveChangesAsync(); // Save changes
        return true;
    }


    // Get all users in a group
    public async Task<List<User>> GetUsersByGroupAsync(int group)
    {
        return await _context.Users.Where(u => u.Group == group).ToListAsync(); // 
    }
    
    // Get all users
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }
    // Get all the users in the database and put each user into an array based on their group.
    public async Task<Dictionary<int, List<User>>> GetUsersByGroupAsync() // Get all users in the database and group them by group number
    {
        var users = await _context.Users
        .Include(u => u.TimeLogs) // Eagerly load the TimeLogs for each user
        .ThenInclude(t => t.TimeLogEntries) // Eagerly load the TimeLogEntries for each TimeLog
        .ToListAsync();

        var groups = new Dictionary<int, List<User>>();
        foreach (var user in users) // Loop through each user
        {
            // Console.WriteLine($"User: {user.NetID}, Group: {user.Group}");
            if (!groups.ContainsKey(user.Group))
            {
                groups[user.Group] = new List<User>(); // Create a new list for the group if it doesn't exist
            }
            groups[user.Group].Add(user); // Add the user to the appropriate group list
        }
        
        foreach (var group in groups) // Loop through each group and print the users in the group
        {
            Console.WriteLine($"Group: {group.Key}");
            foreach (var user in group.Value) // Loop through each user in the group
            {
                Console.WriteLine($"User: {user.NetID}, FirstName: {user.FirstName}, LastName: {user.LastName}");
            }
        }
        
        return groups;
    }

        // Method to check if two users are in the same group
    public async Task<bool> AreUsersInSameGroupAsync(string reviewerNetId, string revieweeNetId)
    {
        var reviewer = await GetUserByNetIdAsync(reviewerNetId);
        var reviewee = await GetUserByNetIdAsync(revieweeNetId);

        if (reviewer == null || reviewee == null)
        {
            return false; // Either user not found
        }

        return reviewer.Group == reviewee.Group;
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