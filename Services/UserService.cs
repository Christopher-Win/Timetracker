using TimeTracker.Models; // References application models, including the User model
using Microsoft.AspNetCore.Http; // Provides classes for handling HTTP requests, such as file uploads
using Microsoft.AspNetCore.Identity; // Enables Identity-related functionality (not directly used here)
using Microsoft.EntityFrameworkCore; // Provides ORM functionality for database access
using System.Security.Cryptography; // Provides cryptographic services for hashing
using TimeTracker.Services; // References other service interfaces and implementations
using TimeTracker.Data; // References the application's database context
using System.Text; // Enables text encoding for password hashing

// Service for managing user-related operations
public class UserService : IUserService
{
    private readonly ApplicationDBContext _context; // Database context for accessing and managing user data

    // Constructor for injecting the database context
    public UserService(ApplicationDBContext context)
    {
        _context = context; // Assigns the injected database context to a private field
    }

    // Retrieves a User object by their unique NetID
    public async Task<User> GetUserByNetIdAsync(string netId)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.NetID == netId); // Fetches the user from the database
        if (user == null)
        {
            throw new KeyNotFoundException("User not found."); // Throws an exception if the user is not found
        }
        return user; // Returns the User object
    }

    // Imports user data from a file and creates user accounts in the database
    public async Task ImportUsersFromFileAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream(); // Opens a stream to read the uploaded file
        using var reader = new StreamReader(stream); // Creates a StreamReader for reading the file
        var users = new List<User>(); // Prepares a list to hold the new users

        bool isFirstLine = true; // Used to skip the header row in the file

        while (!reader.EndOfStream) // Loops until the end of the file is reached
        {
            var line = await reader.ReadLineAsync(); // Reads a line from the file
            line = line.Trim(); // Removes leading and trailing whitespace, including newline characters

            if (isFirstLine) // Skips the header row
            {
                isFirstLine = false;
                continue;
            }

            var values = line.Split('\t'); // Splits the line into values based on tab delimiters
            Console.WriteLine($"LastName: {values[0].Trim()}, FirstName: {values[1].Trim()}, NetID: {values[2].Trim()}, UTDID: {values[3].Trim()}, Group: {values[4].Trim()}");

            if (values.Length != 5) // Checks if the line contains the expected number of values
            {
                continue; // Skips invalid lines
            }

            // Extracts values from the line and trims any extra whitespace
            var lastName = values[0].Trim();
            var firstName = values[1].Trim();
            var netId = values[2].Trim(); // Username (NetID)
            var utdId = values[3].Trim(); // Student ID (UTDID)
            var group = values[4].Trim(); // Group number

            // Checks if the user already exists in the database
            if (await _context.Users.AnyAsync(u => u.NetID == netId))
            {
                continue; // Skips duplicates
            }

            // Creates a new User object
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                NetID = netId,
                Password = HashPassword(utdId), // Hashes the password using the UTDID
                Role = "Student", // Assigns the default role of "Student"
                Group = int.Parse(group) // Converts the group to an integer
            };

            users.Add(user); // Adds the user to the list
        }

        if (users.Any()) // Checks if there are valid users to import
        {
            _context.Users.AddRange(users); // Adds all users to the database context
            await _context.SaveChangesAsync(); // Saves changes to the database
        }
        else
        {
            throw new InvalidOperationException("No valid users to import."); // Throws an exception if no users were added
        }
    }

    // Updates the group assignment for a user by their NetID
    public async Task<bool> UpdateUserGroupAsync(string netId, int group)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.NetID == netId); // Finds the user by their NetID
        if (user == null)
        {
            return false; // Returns false if the user is not found
        }
        user.Group = group; // Updates the user's group
        await _context.SaveChangesAsync(); // Saves changes to the database
        return true; // Returns true to indicate success
    }

    // Retrieves all users in a specific group
    public async Task<List<User>> GetUsersByGroupAsync(int group)
    {
        // Queries the database for users in the specified group
        return await _context.Users.Where(u => u.Group == group).ToListAsync();
    }

    // Retrieves all users in the database
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync(); // Returns a list of all users
    }

    // Groups all users in the database by their group number
    public async Task<Dictionary<int, List<User>>> GetUsersByGroupAsync()
    {
        // Fetches all users and eagerly loads related entities
        var users = await _context.Users
            .Include(u => u.TimeLogs) // Loads TimeLogs for each user
            .ThenInclude(t => t.TimeLogEntries) // Loads TimeLogEntries for each TimeLog
            .Include(pr => pr.ReviewsGiven) // Loads PeerReviews given by each user
            .ToListAsync();

        var groups = new Dictionary<int, List<User>>(); // Initializes a dictionary to group users

        foreach (var user in users) // Loops through all users
        {
            if (!groups.ContainsKey(user.Group)) // Checks if the group exists in the dictionary
            {
                groups[user.Group] = new List<User>(); // Creates a new list for the group if it doesn't exist
            }
            groups[user.Group].Add(user); // Adds the user to their group's list
        }

        foreach (var group in groups) // Logs group information for debugging
        {
            Console.WriteLine($"Group: {group.Key}");
            foreach (var user in group.Value)
            {
                Console.WriteLine($"User: {user.NetID}, FirstName: {user.FirstName}, LastName: {user.LastName}");
            }
        }

        return groups; // Returns the grouped users
    }

    // Checks if two users belong to the same group
    public async Task<bool> AreUsersInSameGroupAsync(string reviewerNetId, string revieweeNetId)
    {
        var reviewer = await GetUserByNetIdAsync(reviewerNetId); // Fetches the reviewer
        var reviewee = await GetUserByNetIdAsync(revieweeNetId); // Fetches the reviewee

        if (reviewer == null || reviewee == null)
        {
            return false; // Returns false if either user is not found
        }

        return reviewer.Group == reviewee.Group; // Returns true if both users are in the same group
    }

    // Hashes a password using SHA-256
    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create()) // Creates a new SHA-256 instance
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password)); // Hashes the password
            var builder = new StringBuilder(); // Converts the hash bytes to a hexadecimal string
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2")); // Appends each byte as a two-digit hex value
            }
            return builder.ToString(); // Returns the hashed password
        }
    }
}