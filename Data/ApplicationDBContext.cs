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
        public DbSet<TimeLog> TimeLogs { get; set; }
        public DbSet<TimeLogEntry> TimeLogEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // One-to-many relationship between User and TimeLog
            modelBuilder.Entity<User>()
                .HasMany(u => u.TimeLogs)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId);

            // One-to-many relationship between TimeLog and TimeLogEntry
            modelBuilder.Entity<TimeLog>()
                .HasMany(t => t.TimeLogEntries)
                .WithOne(e => e.TimeLog)
                .HasForeignKey(e => e.TimeLogId);
           
        }
    }
}