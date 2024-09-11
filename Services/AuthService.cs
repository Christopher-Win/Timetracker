using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using TimeTracker.Models;
using TimeTracker.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace TimeTracker.Services{
    public class AuthService(ApplicationDBContext context)
    {
        private readonly ApplicationDBContext _context = context;

        public User? GetById(string id)
        {
            var user = _context.Users.FirstOrDefault(u => u.NetID == id);

            return user != null ? user : null;
        }
        
        public async Task<Result> RegisterAsync(User newUser)
        {
            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.NetID == newUser.NetID);
            if (existingUser != null)
            {
                return new Result { Success = false, Message = "NetID is already taken." };
            }
        
            var hashedPassword = HashPassword(newUser.Password); // Hash the password before saving it to the database
        
            var user = new User
            {
                NetID = newUser.NetID,
                Password = hashedPassword,
                CreatedAt = DateTime.Now
            };
        
            _context.Users.Add(user);   // Add the user to the database
            await _context.SaveChangesAsync();  // Save the changes to the database
        
            return new Result { Success = true }; // Return a success message once the new user has been saved to the database
        }
        // public async Task<Result> AuthenticateUser(string netID, string password) // This method will open a connection to the database and check if the user exists when a user tries to log in.
        // {
        //     using (var connection = new MySqlConnection(_connectionString)) // Create a MySQL connection to the database for th duration of the using block
        //     {
        //         await connection.OpenAsync(); // Open the connection to the database

        //         string query = "SELECT * FROM Users WHERE netID = @netID AND Password = @Password"; // Create a query to check if the user exists
        //         using (var command = new MySqlCommand(query, connection)) // Create a MySQL command to execute the query on the current database connection
        //         {
        //             command.Parameters.AddWithValue("@netID", netID); 
        //             command.Parameters.AddWithValue("@Password",  HashPassword(password));

        //              using (var reader = await command.ExecuteReaderAsync())// The reader will read the result of the query and return the first row
        //             {
        //                 if (await reader.ReadAsync()) // If the reader has rows, it will return true
        //                 {
        //                 //     return new User // 
        //                 //     {
        //                 //         netID = reader.GetString("netID"),
        //                 //         Password = reader.GetString("Password"),
        //                 //     };
        //                 // }
                        
        //                     return new Result { Success = true, Message = "User Logged in successfully" };

        //                 }
        //             }
        //         }
        //     return null;
        //     }
        // }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public class Result
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }

    }
}