// File: Data/ApplicationDBContext.cs

using Microsoft.EntityFrameworkCore;
using TimeTracker.Models;  // Make sure to adjust this namespace for your project

namespace TimeTracker.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }   // Example model
        // public DbSet<TimeLog> TimeLogs { get; set; }   // Example model

        // Additional DbSet properties for other models
    }
}