// Written by: Chris N.
using Microsoft.EntityFrameworkCore; // Provides the Entity Framework Core ORM for database operations
using TimeTracker.Models; // References application models for entities like User, TimeLog, etc.

namespace TimeTracker.Data
{
    // Represents the database context for the application
    // This class is used by Entity Framework Core to interact with the database
    public class ApplicationDBContext : DbContext
    {
        // Constructor for injecting database context options
        // DbContextOptions provides configuration details like the database provider and connection string
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }

        // DbSets represent tables in the database and provide an API to query and manage the data
        public DbSet<User> Users { get; set; } // Represents the Users table
        public DbSet<TimeLog> TimeLogs { get; set; } // Represents the TimeLogs table
        public DbSet<TimeLogEntry> TimeLogEntries { get; set; } // Represents the TimeLogEntries table
        public DbSet<PeerReview> PeerReviews { get; set; } // Represents the PeerReviews table
        public DbSet<PeerReviewQuestion> PeerReviewQuestions { get; set; } // Represents predefined peer review questions
        public DbSet<PeerReviewAnswer> PeerReviewAnswers { get; set; } // Represents answers to peer review questions

        // Configures relationships and constraints between entities
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure NetID as an alternate key for the User entity
            // Ensures that NetID is unique and can be used as a lookup key
            modelBuilder.Entity<User>()
                .HasAlternateKey(u => u.NetID);

            // Define a one-to-many relationship between User and TimeLog
            // Each User can have multiple TimeLogs
            modelBuilder.Entity<User>()
                .HasMany(u => u.TimeLogs) // Specifies the collection of TimeLogs in the User entity
                .WithOne(t => t.User) // Each TimeLog is associated with one User
                .HasForeignKey(t => t.UserId); // Foreign key in TimeLog points to User

            // Define a one-to-many relationship between TimeLog and TimeLogEntry
            // Each TimeLog can have multiple TimeLogEntries
            modelBuilder.Entity<TimeLog>()
                .HasMany(t => t.TimeLogEntries) // Specifies the collection of TimeLogEntries in the TimeLog entity
                .WithOne(e => e.TimeLog) // Each TimeLogEntry is associated with one TimeLog
                .HasForeignKey(e => e.TimeLogId); // Foreign key in TimeLogEntry points to TimeLog

            // Define Peer Review relationships
            // A User (Reviewer) can give multiple PeerReviews
            modelBuilder.Entity<PeerReview>()
                .HasOne(pr => pr.Reviewer) // Each PeerReview has a single Reviewer
                .WithMany(u => u.ReviewsGiven) // A User can give multiple reviews
                .HasForeignKey(pr => pr.ReviewerId) // Foreign key in PeerReview points to Reviewer's NetID
                .HasPrincipalKey(u => u.NetID) // Specifies that NetID is the principal key
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete to avoid accidental data loss

            // A User (Reviewee) can receive multiple PeerReviews
            modelBuilder.Entity<PeerReview>()
                .HasOne(pr => pr.Reviewee) // Each PeerReview is for a single Reviewee
                .WithMany(u => u.ReviewsReceived) // A User can receive multiple reviews
                .HasForeignKey(pr => pr.RevieweeId) // Foreign key in PeerReview points to Reviewee's NetID
                .HasPrincipalKey(u => u.NetID) // Specifies that NetID is the principal key
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete to avoid accidental data loss

            // Define relationship between PeerReview and PeerReviewAnswer
            // Each PeerReview can have multiple PeerReviewAnswers
            modelBuilder.Entity<PeerReviewAnswer>()
                .HasOne(pra => pra.PeerReview) // Each answer is associated with one PeerReview
                .WithMany() // PeerReview does not explicitly define a collection of answers
                .HasForeignKey(pra => pra.PeerReviewId); // Foreign key in PeerReviewAnswer points to PeerReview

            // Define relationship between PeerReviewAnswer and PeerReviewQuestion
            // Each PeerReviewAnswer is associated with one PeerReviewQuestion
            modelBuilder.Entity<PeerReviewAnswer>()
                .HasOne(pra => pra.PeerReviewQuestion) // Each answer corresponds to one question
                .WithMany() // PeerReviewQuestion does not explicitly define a collection of answers
                .HasForeignKey(pra => pra.PeerReviewQuestionId); // Foreign key in PeerReviewAnswer points to PeerReviewQuestion
        }
    }
}